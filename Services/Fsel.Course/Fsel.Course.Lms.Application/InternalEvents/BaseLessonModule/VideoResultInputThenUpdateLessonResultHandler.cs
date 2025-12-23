// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.InternalEvents.BaseLessonModule
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Core.Applications.InternalEvents;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Entities.SkillScoresConfigs;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Lms.Application.Services.ApplicationServices.CacheServices;
    using Fsel.Course.Lms.Application.Services.ApplicationServices.LearningServices.LessonItemServices;
    using Fsel.Shared.Helpers;
    using MediatR;
    using Microsoft.EntityFrameworkCore;
    using static Fsel.Shared.Constants.ValueSettings;

    public class VideoResultInputThenUpdateLessonResultHandler : BaseLessonResultEventHandler, INotificationHandler<EntityChangedEvent<VideoResult>>
    {
        private readonly ILessonResultRepository _lessonResultRepository;
        private readonly ILessonModuleRepository _lessonModuleRepository;
        private readonly IVideoResultRepository _videoResultRepository;
        private readonly IVideoCachingService _videoCachingService;
        private readonly IVideoRepository _videoRepository;

        public VideoResultInputThenUpdateLessonResultHandler(ILessonResultRepository lessonResultRepository,
            ILessonModuleRepository lessonModuleRepository,
            ILessonModuleCachingService lessonModuleCachingService,
            IClassForumResultRepository classForumResultRepository,
            IHomeWorkResultRepository homeWorkResultRepository,
            IVideoResultRepository videoResultRepository,
            IDocumentResultRepository documentResultRepository,
            ILessonItemInitializerFactory lessonItemInitializerFactory,
            IVideoCachingService videoCachingService,
            IVideoRepository videoRepository) : base(lessonResultRepository, lessonModuleRepository, lessonModuleCachingService, classForumResultRepository, homeWorkResultRepository, videoResultRepository, documentResultRepository, lessonItemInitializerFactory)
        {
            _lessonResultRepository = lessonResultRepository;
            _lessonModuleRepository = lessonModuleRepository;
            _videoResultRepository = videoResultRepository;
            _videoCachingService = videoCachingService;
            _videoRepository = videoRepository;
        }

        public async Task Handle(EntityChangedEvent<VideoResult> notification, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(notification);
            var videoResult = notification.Data;
            try
            {
                var lessonResult = await _lessonResultRepository.GetByIdAsync(videoResult.LessonResultId);
                if (lessonResult == null || !videoResult.LessonModuleId.HasValue || videoResult.Status != EnumResultStatus.Done)
                {
                    return;
                }

                var percentModule = await _lessonModuleRepository.ReadQueryable
                                                    .Where(x => x.Id == videoResult.LessonModuleId)
                                                    .Select(x => x.Percent)
                                                    .FirstOrDefaultAsync(cancellationToken);
                if (lessonResult.Status != EnumResultStatus.Done)
                {
                    await UpdateVideoResultAsync(videoResult, percentModule);
                }

                await UpdateLessonResultAsync(lessonResult, videoResult.LessonModuleId.Value, cancellationToken);
            }
            catch
            {
            }
        }

        private async Task UpdateVideoResultAsync(VideoResult videoResult, double percentModule)
        {
            if (videoResult == null)
            {
                return;
            }

            var video = await GetVideoAsync(videoResult.VideoId).ConfigureAwait(false);
            if (video == null)
            {
                return;
            }

            // Đảm bảo đã có cấu hình trọng số theo từng Type
            EnsureVideoPercentConfigs(video);

            var configs = video.VideoPercentConfigs?
                            .ToDictionary(x => x.Type, x => x.Percent)
                         ?? new Dictionary<EnumTimeCodeType, double>();

            var videoSkillScores = videoResult.VideoSkillScores ?? new List<VideoSkillScores>();

            double totalPercent = 0;

            foreach (var item in videoSkillScores)
            {
                if (item.SkillScores == null || !item.SkillScores.Any())
                {
                    continue;
                }

                // Lấy trọng số Type, nếu không có thì bỏ qua
                if (!configs.TryGetValue(item.Type, out var weightPercent) || weightPercent <= 0)
                {
                    continue;
                }

                var correctCount = item.SkillScores.Sum(x => x.CorrectCount);
                var correctTotal = item.SkillScores.Sum(x => x.TotalCount);

                var typeCorrectPercent = NumberHelper.GetPercent(correctCount, correctTotal, 2);

                var typeWeightedPercent = NumberHelper.ConvertDoublePercent(typeCorrectPercent * weightPercent, 2);
                totalPercent += typeWeightedPercent;
            }

            videoResult.Percent = totalPercent;
            videoResult.PercentModule = NumberHelper.ConvertDoublePercent(totalPercent * percentModule, 2);
            await _videoResultRepository.BulkUpdateList(new List<VideoResult> { videoResult },
            bulk =>
            {
                bulk.ColumnInputExpression = entity => new
                {
                    entity.Percent,
                    entity.PercentModule
                };
            });
        }

        private async Task<Video?> GetVideoAsync(Guid id)
        {
            return await _videoCachingService.GetOrSetAsync(id.ToString(), async (ctx, _) =>
            {
                var video = await _videoRepository.ReadQueryable
                                                  .Where(x => x.Id == id)
                                                  .Include(v => v.VideoTimeCodes)
                                                  .FirstOrDefaultAsync(_);

                return video;
            });
        }

        private static void EnsureVideoPercentConfigs(Video video)
        {
            // Lấy các type đang dùng trong timecode
            var activeTypes = video.VideoTimeCodes
                .Select(t => t.TimeCodeType) // EnumTimeCodeType
                .Distinct()
                .ToList();

            // Không có timecode nào thì thôi
            if (!activeTypes.Any())
            {
                video.VideoPercentConfigs = new List<VideoPercentConfig>();
                return;
            }

            // Nếu đã có config rồi thì không động vào (user đã chỉnh tay)
            if (video.VideoPercentConfigs != null && video.VideoPercentConfigs.Any())
            {
                return;
            }

            var configs = new List<VideoPercentConfig>();

            if (activeTypes.Count == 1)
            {
                // 1 type -> 100%
                configs.Add(new VideoPercentConfig
                {
                    Type = activeTypes[0],
                    Percent = VideoPercentDefaults.SingleTypePercent
                });
            }
            else if (activeTypes.Count == 2)
            {
                // 2 type -> 50 - 50
                foreach (var type in activeTypes)
                {
                    configs.Add(new VideoPercentConfig
                    {
                        Type = type,
                        Percent = VideoPercentDefaults.TwoTypesPercentEach
                    });
                }
            }
            else
            {
                // 3 type trở lên:
                // rule đặc biệt cho 3 thằng Standalone, UnitTest, SkillTest: 34-33-33
                foreach (var type in activeTypes)
                {
                    var percent = type switch
                    {
                        EnumTimeCodeType.Standalone => VideoPercentDefaults.Standalone,
                        EnumTimeCodeType.UnitTest => VideoPercentDefaults.UnitTest,
                        EnumTimeCodeType.SkillTest => VideoPercentDefaults.SkillTest,
                        _ => VideoPercentDefaults.DefaultUnknownTypePercent
                    };

                    configs.Add(new VideoPercentConfig
                    {
                        Type = type,
                        Percent = percent
                    });
                }

                // Nếu có type khác ngoài 3 thằng trên thì có thể xử lý thêm ở đây nếu cần.
            }

            // Đảm bảo tổng = 100 (nếu lệch do rule, cộng dồn vào phần tử cuối)
            var total = configs.Sum(x => x.Percent);
            var diff = VideoPercentDefaults.TotalPercent - total;
            if (Math.Abs(diff) > VideoPercentDefaults.Epsilon && configs.Count > 0)
            {
                configs[^1].Percent += diff;
            }

            video.VideoPercentConfigs = configs;
        }
    }
}
