// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Services.ApplicationServices.LearningServices.LessonItemServices
{
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Entities.V1i1;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Infrastructure.Repositories;
    using Fsel.Shared.ApplicationServices.CacheServices;
    using Microsoft.EntityFrameworkCore;

    public class VideoLessonItemInitializer : ILessonItemInitializer
    {
        private readonly IVideoRepository _videoRepository;
        private readonly IVideoResultRepository _videoResultRepository;
        private readonly IRequestSafeCachingService _requestSafeCachingService;

        public VideoLessonItemInitializer(IVideoRepository videoRepository,
            IVideoResultRepository videoResultRepository,
            IRequestSafeCachingService requestSafeCachingService)
        {
            _videoRepository = videoRepository;
            _videoResultRepository = videoResultRepository;
            _requestSafeCachingService = requestSafeCachingService;
        }

        public async Task<VoidMethodResult> InitializeAsync(LessonModule lessonModule, LessonResult lessonResult, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(lessonModule);
            ArgumentNullException.ThrowIfNull(lessonResult);
            var methodResult = new VoidMethodResult();
            if (lessonModule.LessonConfigType != EnumLessonConfigType.Video)
            {
                return methodResult;
            }

            var videoResult = await _videoResultRepository.Queryable.Where(x => x.LessonResultId == lessonResult.Id)
                                                          .Where(x => x.LessonModuleId == lessonModule.Id)
                                                          .FirstOrDefaultAsync(cancellationToken);
            if (videoResult != null)
            {
                if (videoResult.Status == EnumResultStatus.Unfinished)
                {
                    videoResult.Status = EnumResultStatus.New;
                    videoResult.NewDate = DateTime.UtcNow;
                    await _videoResultRepository.UnitOfWork.SaveChangesAsync(cancellationToken);
                }
                return methodResult;
            }

            var video = await _videoRepository.ReadQueryable.Where(x => x.OriginalId == lessonModule.OriginalId)
                                         .Where(x => x.VersionStatus == EnumVersionStatus.LastVersion)
                                         .FirstOrDefaultAsync(cancellationToken);

            if (video == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(video), lessonModule.OriginalId);
                return methodResult;
            }

            videoResult = new VideoResult
            {
                LessonModuleId = lessonModule.Id,
                LessonResultId = lessonResult.Id,
                StudentId = lessonResult.StudentId,
                Status = EnumResultStatus.New,
                NewDate = DateTime.UtcNow,
                VideoId = video.Id,
            };

            await _requestSafeCachingService.SafeRequest(
                key: $"Add_VideoResult_{videoResult.LessonModuleId}_{videoResult.LessonResultId}_{videoResult.IsDeleted}",
                safeFunction: async () =>
                {
                    await _videoResultRepository.BulkMergeAsync(new List<VideoResult> { videoResult }, bulk =>
                    {
                        bulk.ColumnPrimaryKeyExpression = c => new { c.LessonModuleId, c.LessonResultId, c.IsDeleted };
                    });
                    return videoResult;
                });

            return methodResult;
        }
    }
}
