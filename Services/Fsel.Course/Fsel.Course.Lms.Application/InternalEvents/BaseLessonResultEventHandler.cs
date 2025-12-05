// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.InternalEvents
{
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Entities.V1i1;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Lms.Application.Services.ApplicationServices.CacheServices;
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

        public BaseLessonResultEventHandler(ILessonResultRepository lessonResultRepository,
            ILessonModuleRepository lessonModuleRepository,
            ILessonModuleCachingService lessonModuleCachingService,
            IClassForumResultRepository classForumResultRepository,
            IHomeWorkResultRepository homeWorkResultRepository,
            IVideoResultRepository videoResultRepository,
            IDocumentResultRepository documentResultRepository)
        {
            _lessonResultRepository = lessonResultRepository;
            _lessonModuleRepository = lessonModuleRepository;
            _lessonModuleCachingService = lessonModuleCachingService;
            _classForumResultRepository = classForumResultRepository;
            _homeWorkResultRepository = homeWorkResultRepository;
            _videoResultRepository = videoResultRepository;
            _documentResultRepository = documentResultRepository;
        }

        public async Task UpdateLessonResultAsync(LessonResult lessonResult, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(lessonResult);
            // Tính toán lại tiến độ hoàn thành bài học
            var lessonModules = await GetLessonModulesAsync(lessonResult.Id);
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

                // Giả sử mỗi loại kết quả tương ứng với một module hoàn thành
            }
            // Lưu thay đổi
            _lessonResultRepository.Update(lessonResult);
            await _lessonResultRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
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
