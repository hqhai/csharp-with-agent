// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.InternalEvents
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Core.Applications.InternalEvents;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Entities.V1i1;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Lms.Application.Services.ApplicationServices.CacheServices;
    using Fsel.Course.Lms.Application.Services.LessonItemServices;
    using Fsel.Shared.Helpers;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class VideoResultInputThenUpdateLessonResultHandler : INotificationHandler<EntityChangedEvent<VideoResult>>
    {
        private readonly ILessonResultRepository _lessonResultRepository;
        private readonly ILessonModuleRepository _lessonModuleRepository;
        private readonly ILessonModuleCachingService _lessonModuleCachingService;
        private readonly ILessonItemInitializerFactory _lessonItemInitializerFactory;

        public VideoResultInputThenUpdateLessonResultHandler(ILessonResultRepository lessonResultRepository,
            ILessonModuleRepository lessonModuleRepository,
            ILessonModuleCachingService lessonModuleCachingService,
            ILessonItemInitializerFactory lessonItemInitializerFactory)
        {
            _lessonResultRepository = lessonResultRepository;
            _lessonModuleRepository = lessonModuleRepository;
            _lessonModuleCachingService = lessonModuleCachingService;
            _lessonItemInitializerFactory = lessonItemInitializerFactory;
        }

        public async Task Handle(EntityChangedEvent<VideoResult> notification, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(notification);
            var videoResult = notification.Data;
            try
            {
                var lessonResult = videoResult.LessonResult;
                if (lessonResult == null || videoResult.Status != EnumResultStatus.Done)
                {
                    return;
                }

                var lessonModules = await GetLessonModulesAsync(lessonResult.LessonId);

                // 2. Xác định module hiện tại tương ứng với VideoResult
                var currentModule = FindCurrentModule(lessonModules, videoResult);
                if (currentModule == null)
                {
                    // Không map được module hiện tại -> không làm gì
                    return;
                }

                // 3. Lấy module tiếp theo
                var nextModule = GetNextModule(lessonModules, currentModule);
                if (nextModule != null)
                {
                    var initializer = _lessonItemInitializerFactory.Get(nextModule.LessonConfigType);
                    if (initializer != null)
                    {
                        await initializer.InitializeAsync(nextModule, lessonResult, cancellationToken);
                    }
                    return;
                }

                var skillScores = videoResult.VideoSkillScores?.FirstOrDefault(x => x.Type == EnumTimeCodeType.Standalone)?.SkillScores;
                lessonResult.CorrectCount = videoResult.CorrectCount;
                lessonResult.CorrectTotal = videoResult.CorrectTotal;
                lessonResult.Percent = NumberHelper.ConvertDoublePercent(videoResult.Percent * 40);
                lessonResult.SkillScores = skillScores;

                await _lessonResultRepository.BulkUpdateList(new List<LessonResult> { lessonResult }, bulk =>
                {
                    bulk.IgnoreOnUpdateExpression = c => new { c.CourseId, c.StudentId, c.LessonId, c.UnitId };
                });
            }
            catch
            {
            }
        }

        private static LessonModule? FindCurrentModule(IList<LessonModule> lessonModules, VideoResult videoResult)
        {
            return lessonModules.FirstOrDefault(m => m.Id == videoResult.LessonModuleId);
        }

        private static LessonModule? GetNextModule(IList<LessonModule> lessonModules, LessonModule currentModule)
        {
            return lessonModules.Where(m => m.OpenOrder > currentModule.OpenOrder)
                                .OrderBy(m => m.OpenOrder)
                                .FirstOrDefault();
        }

        private async Task<IList<LessonModule>> GetLessonModulesAsync(Guid id)
        {
            return await _lessonModuleCachingService.GetOrSetAsync(id.ToString(), async (ctx, _) =>
            {
                var lessonModules = await _lessonModuleRepository.ReadQueryable
                                                             .Where(x => x.LessonId == id)
                                                             .ToListAsync(_);

                return lessonModules.OrderBy(x => x.DisplayOrder).ToList();
            });
        }
    }
}
