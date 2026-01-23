// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Common
{
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Entities.SkillScoresConfigs;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Helpers;
    using Microsoft.EntityFrameworkCore;

    public class ChangeCourseHelper
    {
        private readonly ICourseResultRepository _courseResultRepository;
        private readonly IUnitResultRepository _unitResultRepository;
        private readonly IMockTestResultRepository _mockTestResultRepository;
        private readonly ICourseUnitMockTestRepository _courseUnitMockTestRepository;
        private const int MaxPercent = 67;
        private const int MaxUnitDoneToStop = 3;

        public ChangeCourseHelper(ICourseResultRepository courseResultRepository, IUnitResultRepository unitResultRepository, IMockTestResultRepository mockTestResultRepository, ICourseUnitMockTestRepository courseUnitMockTestRepository)
        {
            _courseResultRepository = courseResultRepository;
            _unitResultRepository = unitResultRepository;
            _mockTestResultRepository = mockTestResultRepository;
            _courseUnitMockTestRepository = courseUnitMockTestRepository;
        }

        public async Task<bool?> IsStudentsAchieveScoresAsync(Guid studentId, EnumCourseLevel? baseCourseLevel)
        {
            var courseResult = await _courseResultRepository.Queryable.Where(x => x.Status == EnumResultStatus.Done && x.StudentId == studentId)
                                                                      .Where(x => x.Course != null && (x.Course.CourseLevel == baseCourseLevel || x.Course.CourseLevel == baseCourseLevel.GetLevelIELTSToAca()))
                                                                      .OrderByDescending(x => x.CreatedDate)
                                                                      .FirstOrDefaultAsync();
            bool? isStudentsAchieveScores;
            if (courseResult != null && courseResult.Status == EnumResultStatus.Done)
            {
                isStudentsAchieveScores = courseResult.Percent > MaxPercent;
                if (isStudentsAchieveScores.HasValue && !isStudentsAchieveScores.Value && courseResult.Course != null && courseResult.Course.CourseType == EnumCourseType.Ielts)
                {
                    isStudentsAchieveScores = await CheckScoreFullMockTestTwoAsync(courseResult);
                }
                return isStudentsAchieveScores;
            }
            return default;
        }

        private async Task<bool> CheckScoreFullMockTestTwoAsync(CourseResult courseResult)
        {
            var courseUnitMockTest = await _courseUnitMockTestRepository.Queryable.Where(x => x.CourseId == courseResult.CourseId && x.MockTestId.HasValue)
                .OrderByDescending(x => x.Number)
                .FirstOrDefaultAsync();
            if (courseUnitMockTest == null)
            {
                return false;
            }
            var mockTestResult = await _mockTestResultRepository.Queryable.Where(x => x.MockTestId == courseUnitMockTest.MockTestId && x.StudentId == courseResult.StudentId)
                                                                        .FirstOrDefaultAsync(x => x.CourseId == courseResult.CourseId && x.Status == EnumResultStatus.Done);
            if (mockTestResult == null)
            {
                return false;
            }
            return GetBandScore(mockTestResult.SkillScores) > TargetBandScoreHelper.GetBandScore(courseResult.Course?.CourseLevel ?? default);
        }

        public double GetBandScore(IList<SkillScores>? skillScores)
        {
            if (skillScores == null || !skillScores.Any())
            {
                return default;
            }
            return skillScores.Sum(x => x.Scores) > 0 ? NumberHelper.RoundNumberDouble(skillScores.Average(x => x.Scores)) : default;
        }

        public async Task<bool> CheckChangeLevelAllCourseAsync(Guid studentId)
        {
            var courseResultActive = await _courseResultRepository.Queryable.FirstOrDefaultAsync(x => x.StudentId == studentId && x.WorkingStatus == EnumWorkingStatus.Active);
            if (courseResultActive != null && courseResultActive.Status != EnumResultStatus.Done)
            {
                var groupUnitResultStatus = await _unitResultRepository.Queryable.Where(x => x.CourseResultId == courseResultActive.Id).GroupBy(x => x.Status)
                       .Select(x => new
                       {
                           StatusResult = x.Key,
                           NumberOfStatus = x.Count(),
                       }).ToListAsync();
                return (groupUnitResultStatus.Any(x => x.StatusResult == EnumResultStatus.Done && x.NumberOfStatus >= MaxUnitDoneToStop - 1) && groupUnitResultStatus.Any(x => x.StatusResult == EnumResultStatus.Process)) ||
                    groupUnitResultStatus.Any(x => x.StatusResult == EnumResultStatus.Done && x.NumberOfStatus >= MaxUnitDoneToStop);
            }
            return false;
        }
    }
}
