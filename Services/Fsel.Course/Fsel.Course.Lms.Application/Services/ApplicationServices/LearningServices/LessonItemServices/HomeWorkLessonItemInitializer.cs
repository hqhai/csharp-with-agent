// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Services.ApplicationServices.LearningServices.LessonItemServices
{
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Entities.SkillScoresConfigs;
    using Fsel.Course.Domain.Entities.V1i1;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Microsoft.EntityFrameworkCore;

    public class HomeWorkLessonItemInitializer : ILessonItemInitializer
    {
        private readonly IHomeWorkRepository _homeWorkRepository;
        private readonly IHomeWorkResultRepository _homeWorkResultRepository;

        public HomeWorkLessonItemInitializer(IHomeWorkRepository homeWorkRepository, IHomeWorkResultRepository homeWorkResultRepository)
        {
            _homeWorkRepository = homeWorkRepository;
            _homeWorkResultRepository = homeWorkResultRepository;
        }

        public async Task<VoidMethodResult> InitializeAsync(LessonModule lessonModule, LessonResult lessonResult, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(lessonModule);
            ArgumentNullException.ThrowIfNull(lessonResult);
            var methodResult = new VoidMethodResult();
            if (lessonModule.LessonConfigType != EnumLessonConfigType.HomeWork)
            {
                return methodResult;
            }

            var homeWorkResult = await _homeWorkResultRepository.Queryable.Where(x => x.LessonResultId == lessonResult.Id)
                                                                .Where(x => x.LessonModuleId == lessonModule.Id)
                                                                .FirstOrDefaultAsync(cancellationToken);
            if (homeWorkResult != null)
            {
                if (homeWorkResult.Status == EnumResultStatus.Unfinished)
                {
                    homeWorkResult.Status = EnumResultStatus.New;
                    homeWorkResult.NewDate = DateTime.UtcNow;
                    await _homeWorkResultRepository.UnitOfWork.SaveChangesAsync(cancellationToken);
                }

                return methodResult;
            }

            var homeWork = await _homeWorkRepository.ReadQueryable.Where(x => x.OriginalId == lessonModule.OriginalId)
                .Include(x => x.HomeWorkQuestions)
                .Include(x => x.Skill)
                .Where(x => x.VersionStatus == EnumVersionStatus.LastVersion)
                .FirstOrDefaultAsync(cancellationToken);

            if (homeWork == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(homeWork), lessonModule.OriginalId);
                return methodResult;
            }

            homeWorkResult = new HomeWorkResult
            {
                LessonResultId = lessonResult.Id,
                StudentId = lessonResult.StudentId,
                Status = EnumResultStatus.New,
                NewDate = DateTime.UtcNow,
                HomeWorkId = homeWork.Id,
                LessonModuleId = lessonModule.Id,
                SkillScores = new List<SkillScores>
                {
                    new SkillScores
                    {
                        Skill = homeWork.CourseSkill,
                        SkillId = homeWork.SkillId,
                        SkillName = homeWork.Skill?.Name,
                        TotalQuestion = homeWork.HomeWorkQuestions?.Count ?? 0,
                    },
                },
            };
            await _homeWorkResultRepository.BulkMergeAsync(new List<HomeWorkResult> { homeWorkResult }, bulk =>
            {
                bulk.ColumnPrimaryKeyExpression = c => new { c.LessonResultId, c.LessonModuleId, c.IsDeleted };
            });
            return methodResult;
        }
    }
}
