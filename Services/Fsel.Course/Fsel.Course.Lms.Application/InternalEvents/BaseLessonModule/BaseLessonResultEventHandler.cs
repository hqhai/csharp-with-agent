// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.InternalEvents.BaseLessonModule
{
    using System.Threading;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Entities.V1i1;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Lms.Application.Services.ApplicationServices.CacheServices;
    using Fsel.Course.Lms.Application.Services.ApplicationServices.LearningServices.LessonItemServices;
    using Microsoft.EntityFrameworkCore;

    public class BaseLessonResultEventHandler
    {
        private readonly ILessonResultRepository _lessonResultRepository;
        private readonly ILessonModuleRepository _lessonModuleRepository;
        private readonly ILessonModuleCachingService _lessonModuleCachingService;
        private readonly IClassForumResultRepository _classForumResultRepository;
        private readonly IHomeWorkResultRepository _homeWorkResultRepository;
        private readonly IVideoResultRepository _videoResultRepository;
        private readonly IDocumentResultRepository _documentResultRepository;
        private readonly ILessonItemInitializerFactory _lessonItemInitializerFactory;

        public BaseLessonResultEventHandler(ILessonResultRepository lessonResultRepository,
            ILessonModuleRepository lessonModuleRepository,
            ILessonModuleCachingService lessonModuleCachingService,
            IClassForumResultRepository classForumResultRepository,
            IHomeWorkResultRepository homeWorkResultRepository,
            IVideoResultRepository videoResultRepository,
            IDocumentResultRepository documentResultRepository,
            ILessonItemInitializerFactory lessonItemInitializerFactory)
        {
            _lessonResultRepository = lessonResultRepository;
            _lessonModuleRepository = lessonModuleRepository;
            _lessonModuleCachingService = lessonModuleCachingService;
            _classForumResultRepository = classForumResultRepository;
            _homeWorkResultRepository = homeWorkResultRepository;
            _videoResultRepository = videoResultRepository;
            _documentResultRepository = documentResultRepository;
            _lessonItemInitializerFactory = lessonItemInitializerFactory;
        }

        public async Task UpdateLessonResultAsync(LessonResult lessonResult, IList<LessonModule> lessonModules, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(lessonResult);
            // Tính toán lại tiến độ hoàn thành bài học
            if (!lessonModules.Any())
            {
                lessonResult.Percent = 100;
            }
            else
            {
                // Kiểm tra các loại kết quả liên quan đến bài học
                var videoResults = await _videoResultRepository.ReadQueryable
                                                               .Where(x => x.LessonResultId == lessonResult.Id && x.Status == EnumResultStatus.Done)
                                                               .ToListAsync(cancellationToken);

                var documentResults = await _documentResultRepository.ReadQueryable
                                                                     .Where(x => x.LessonResultId == lessonResult.Id && x.Status == EnumResultStatus.Done)
                                                                     .ToListAsync(cancellationToken);

                var classForumResults = await _classForumResultRepository.ReadQueryable
                                                                         .Where(x => x.LessonResultId == lessonResult.Id && x.ResultStatus == EnumResultStatus.Done)
                                                                         .ToListAsync(cancellationToken);

                var homeWorkResults = await _homeWorkResultRepository.ReadQueryable
                                                                     .Where(x => x.LessonResultId == lessonResult.Id && x.Status == EnumResultStatus.Done)
                                                                     .ToListAsync(cancellationToken);

                var skillScores = videoResults.SelectMany(x => x.VideoSkillScores)
                                              .Where(x => x.Type == EnumTimeCodeType.Standalone)
                                              .Select(x => x.SkillScores)
                                              .ToList();

                var skillScoreHomeWork = homeWorkResults.SelectMany(x => x.SkillScores)
                                                        .ToList();

                var skillScoreClassForum = classForumResults.SelectMany(x => x.SkillScores).ToList();

                // Giả sử mỗi loại kết quả tương ứng với một module hoàn thành
            }
            // Lưu thay đổi
            _lessonResultRepository.Update(lessonResult);
            await _lessonResultRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        public async Task UpdateLessonResultAsync(LessonResult lessonResult, Guid lessonModuleId, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(lessonResult);
            var lessonModules = await GetLessonModulesAsync(lessonResult.LessonId);
            var currentModule = FindCurrentModule(lessonModules, lessonModuleId);
            if (currentModule == null)
            {
                return;
            }

            var nextModule = GetNextModule(lessonModules, currentModule);
            if (nextModule != null)
            {
                await UpdateNewResultLessonModule(nextModule, lessonResult, cancellationToken);
                return;
            }
            await UpdateLessonResultAsync(lessonResult, lessonModules, cancellationToken);
        }

        private static LessonModule? FindCurrentModule(IList<LessonModule> lessonModules, Guid? lessonModuleId)
        {
            if (!lessonModuleId.HasValue)
            {
                return null;
            }
            return lessonModules.FirstOrDefault(m => m.Id == lessonModuleId);
        }

        private static LessonModule? GetNextModule(IList<LessonModule> lessonModules, LessonModule currentModule)
        {
            return lessonModules.Where(m => m.OpenOrder > currentModule.OpenOrder)
                                .OrderBy(m => m.OpenOrder)
                                .FirstOrDefault();
        }

        private async Task UpdateNewResultLessonModule(LessonModule nextModule, LessonResult lessonResult, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(nextModule);
            var initializer = _lessonItemInitializerFactory.Get(nextModule.LessonConfigType);
            if (initializer != null)
            {
                await initializer.InitializeAsync(nextModule, lessonResult, cancellationToken);
            }
            return;
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
