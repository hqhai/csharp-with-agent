// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Common
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Helpers;
    using Microsoft.EntityFrameworkCore;

    public class ManagerProgressHelper
    {
        private readonly ILessonResultRepository _lessonResultRepository;
        private readonly IFinalTestResultRepository _finalTestResultRepository;
        private readonly ICourseRepository _courseRepository;
        private readonly ICourseResultRepository _courseResultRepository;
        private readonly IUnitRepository _unitRepository;
        private readonly IMockTestResultRepository _mockTestResultRepository;
        private const int NumberModuleLesson = 3;
        private const int NumberModuleSkillMockTest = 1;
        private const int NumberDefaultComplete = 1;
        private const int NumberDefault = 0;

        public ManagerProgressHelper(ILessonResultRepository lessonResultRepository, IFinalTestResultRepository finalTestResultRepository, ICourseRepository courseRepository, ICourseResultRepository courseResultRepository, IUnitRepository unitRepository, IMockTestResultRepository mockTestResultRepository)
        {
            _lessonResultRepository = lessonResultRepository;
            _finalTestResultRepository = finalTestResultRepository;
            _courseRepository = courseRepository;
            _courseResultRepository = courseResultRepository;
            _unitRepository = unitRepository;
            _mockTestResultRepository = mockTestResultRepository;
        }

        public async Task<UnitStudentProgressModel?> GetUnitManager(UnitResult? unitResult, Guid unitId, Guid courseId)
        {
            var unit = await _unitRepository.Queryable
                                            .Include(x => x.UnitLessons)
                                            .Include(x => x.UnitSkillMockTests)
                                            .Include(x => x.CourseUnitMockTests.Where(c => c.CourseId == courseId))
                                            .FirstOrDefaultAsync(x => x.Id == unitId);

            if (unit == null)
            {
                return default;
            }
            var (currentProgress, progress) = await GetContentComplete(unitResult, unit);
            var unitProgress = new UnitStudentProgressModel
            {
                Type = nameof(CourseUnitMockTest.Unit),
                ObjectId = unit.Id,
                Name = unit.Name,
                DisplayOrder = unit.CourseUnitMockTests.Max(x => x.DisplayOrder),
                Status = unitResult?.Status ?? EnumResultStatus.Unfinished,
                CorrectPercent = unitResult?.Percent ?? default,
                ContentProgress = $"{currentProgress} / {progress}",
                TotalLesson = unit.UnitLessons.Count,
                ProcessPercent = NumberHelper.GetPercent(currentProgress, progress)
            };
            return unitProgress;
        }

        public async Task<CourseStudentProgressModel?> GetCourseManagerAsync(Guid courseId, Guid? studentId, CancellationToken cancellationToken)
        {
            var courseResult = await _courseResultRepository.Queryable.Include(x => x.Course).FirstOrDefaultAsync(x => x.StudentId == studentId && x.CourseId == courseId, cancellationToken);
            if (courseResult == null)
            {
                return default;
            }
            var (currentProgress, progress) = await GetContentComplete(new CourseResultModel
            {
                CourseType = courseResult.Course?.CourseType,
                CourseId = courseResult.CourseId,
                StudentId = courseResult.StudentId
            });
            var courseProgress = new CourseStudentProgressModel
            {
                ContentCompleted = string.Format("{0} / {1}", currentProgress, progress),
                StartDate = courseResult.ProcessDate,
                EndDate = courseResult.CompletionDate,
                CourseName = courseResult.Course?.Code,
                CourseId = courseResult.Course?.Id ?? default,
            };
            return courseProgress;
        }

        public async Task<(int, int)> GetContentComplete(UnitResult? unitResult, Unit unit)
        {
            ArgumentNullException.ThrowIfNull(unit);
            var lessonIds = unit.UnitLessons.Select(x => x.LessonId).ToList();
            var mockTestId = unit.UnitSkillMockTests.FirstOrDefault()?.MockTestId;

            var counts = new List<int>();
            var totalModules = new List<int>();
            if (lessonIds != null && lessonIds.Any())
            {
                if (unitResult != null)
                {
                    var totalModuleLesson = await _lessonResultRepository.Queryable
                                                .Include(x => x.ClassForumResults)
                                                .Include(x => x.VideoResult)
                                                .Include(x => x.HomeWorkResults)
                                                .Where(x => x.UnitId == unitResult.UnitId && x.CourseId == unitResult.CourseId)
                                                .Where(x => lessonIds.Contains(x.LessonId) && x.StudentId == unitResult.StudentId)
                                                .AsNoTracking()
                                                .Select(x => new
                                                {
                                                    CountVideo = x.VideoResult != null && x.VideoResult.Status == EnumResultStatus.Done ? NumberDefaultComplete : NumberDefault,
                                                    CountClassForum = x.ClassForumResults.Where(x => x.Status.HasValue).Count(),
                                                    CountHomeWork = x.HomeWorkResults.Any() && x.HomeWorkResults.All(x => x.Status == EnumResultStatus.Done) ? NumberDefaultComplete : NumberDefault,
                                                })
                                                .ToListAsync();

                    counts.AddRange(new List<int> { totalModuleLesson.Sum(x => x.CountVideo), totalModuleLesson.Sum(x => x.CountClassForum), totalModuleLesson.Sum(x => x.CountHomeWork) });
                }
                totalModules.Add(lessonIds.Count * NumberModuleLesson);
            }
            if (mockTestId.HasValue)
            {
                if (unitResult != null)
                {
                    var mockTestResult = await _mockTestResultRepository.Queryable.Where(x => x.CourseId == unitResult.CourseId && x.UnitId == unitResult.UnitId).FirstOrDefaultAsync(x => x.StudentId == unitResult.StudentId && x.MockTestId == mockTestId.Value);
                    counts.Add(mockTestResult?.Status == EnumResultStatus.Done ? NumberModuleSkillMockTest : NumberDefault);
                }
                totalModules.Add(NumberModuleSkillMockTest);
            }
            return (counts.Sum(), totalModules.Sum());
        }

        public async Task<(int, int)> GetContentComplete(CourseResultModel courseResult)
        {
            ArgumentNullException.ThrowIfNull(courseResult);
            var lessonIds = new List<Guid>();
            var counts = new List<int>();
            var countTests = new List<(int, int)>();
            var course = await _courseRepository.Queryable.Include(x => x.CourseUnitMockTests).Where(x => x.Id == courseResult.CourseId).FirstOrDefaultAsync();
            var unitIds = course?.CourseUnitMockTests.Where(x => x.UnitId != null).Select(x => x.UnitId ?? default).ToList();
            if (unitIds != null && unitIds.Any())
            {
                var units = await _unitRepository.Queryable.Include(x => x.UnitResults).Include(x => x.UnitSkillMockTests).Include(x => x.UnitLessons)
                                                           .Where(x => unitIds.Contains(x.Id)).ToListAsync();
                foreach (var unit in units)
                {
                    countTests.Add(await GetContentComplete(unit.UnitResults.FirstOrDefault(), unit));
                }
            }

            if (courseResult.CourseType == EnumCourseType.Academic)
            {
                var finalTestResult = await _finalTestResultRepository.Queryable.Where(x => x.StudentId == courseResult.StudentId && x.CourseId == courseResult.CourseId).FirstOrDefaultAsync();
                countTests.Add((finalTestResult?.Status == EnumResultStatus.Done ? NumberDefaultComplete : NumberDefault, NumberDefaultComplete));
            }
            else
            {
                var mockTestResults = await _mockTestResultRepository.Queryable.Where(x => x.StudentId == courseResult.StudentId && x.UnitId == null && x.CourseId == courseResult.CourseId).ToListAsync();
                countTests.Add((mockTestResults.Where(x => x.Status == EnumResultStatus.Done).Count(), mockTestResults?.Count ?? default));
            }
            return (countTests.Sum(x => x.Item1), countTests.Sum(x => x.Item2));
        }
    }
}
