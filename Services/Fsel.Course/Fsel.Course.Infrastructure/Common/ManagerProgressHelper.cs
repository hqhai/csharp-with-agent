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
        private readonly IUnitResultRepository _unitResultRepository;
        private readonly IFinalTestResultRepository _finalTestResultRepository;
        private readonly ICourseRepository _courseRepository;
        private readonly ICourseResultRepository _courseResultRepository;
        private readonly IUnitRepository _unitRepository;
        private readonly IMockTestResultRepository _mockTestResultRepository;
        private const int NumberModuleLesson = 3;
        private const int NumberDefaultComplete = 1;
        private const int NumberDefault = 0;

        public ManagerProgressHelper(ILessonResultRepository lessonResultRepository, IUnitResultRepository unitResultRepository, IFinalTestResultRepository finalTestResultRepository, ICourseRepository courseRepository, ICourseResultRepository courseResultRepository, IUnitRepository unitRepository, IMockTestResultRepository mockTestResultRepository)
        {
            _lessonResultRepository = lessonResultRepository;
            _unitResultRepository = unitResultRepository;
            _finalTestResultRepository = finalTestResultRepository;
            _courseRepository = courseRepository;
            _courseResultRepository = courseResultRepository;
            _unitRepository = unitRepository;
            _mockTestResultRepository = mockTestResultRepository;
        }

        public async Task<UnitStudentProgressModel?> GetUnitManager(Guid courseId, Guid unitId, Guid? studentId)
        {
            var unitResult = await _unitResultRepository.Queryable.FirstOrDefaultAsync(x => x.UnitId == unitId && x.CourseId == courseId && x.StudentId == studentId);
            var unit = await _unitRepository.Queryable.Include(x => x.UnitLessons)
                                            .Include(x => x.UnitSkillMockTests)
                                            .Include(x => x.CourseUnitMockTests.Where(c => c.CourseId == courseId))
                                            .FirstOrDefaultAsync(x => x.Id == unitId);
            if (unit == null)
            {
                return default;
            }
            var (currentProgress, progress) = await GetUnitCompleteAsync(unit, unitResult);
            var unitProgress = new UnitStudentProgressModel
            {
                Type = nameof(CourseUnitMockTest.Unit),
                ObjectId = unit.Id,
                Name = unit.Name,
                DisplayOrder = unit.CourseUnitMockTests.Where(c => c.CourseId == courseId).Max(x => x.DisplayOrder),
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
            var (currentProgress, progress) = await GetCompleteCourseAsync(new CourseResultModel
            {
                CourseType = courseResult.Course?.CourseType,
                CourseId = courseResult.CourseId,
                StudentId = courseResult.StudentId
            });
            var courseProgress = new CourseStudentProgressModel
            {
                ContentCompleted = string.Format("{0} / {1}", currentProgress, progress),
                StartDate = courseResult.ProcessDate,
                CreatedDate = courseResult.CreatedDate,
                UpdatedDate = courseResult.UpdatedDate,
                EndDate = courseResult.CompletionDate,
                CourseName = courseResult.Course?.Code,
                CourseId = courseResult.Course?.Id ?? default,
            };
            return courseProgress;
        }

        public async Task<(int, int)> GetUnitCompleteAsync(Unit unit, UnitResult? unitResult)
        {
            ArgumentNullException.ThrowIfNull(unit);
            var lessonIds = unit.UnitLessons.Select(x => x.LessonId).ToList();
            var mockTestIds = unit.UnitSkillMockTests.Select(x => x.MockTestId).ToList();
            var counts = new List<int>();
            var totalModules = new List<int>();
            if (lessonIds != null && lessonIds.Any())
            {
                if (unitResult != null)
                {
                    var query = await _lessonResultRepository.Queryable.Include(x => x.VideoResult).Include(x => x.ClassForumResults).Include(x => x.HomeWorkResults)
                                   .Where(x => lessonIds.Contains(x.LessonId) && x.UnitId == unitResult.UnitId && x.CourseId == unitResult.CourseId && x.StudentId == unitResult.StudentId)
                                   .Select(x => new
                                   {
                                       CountVideo = x.VideoResult != null && x.VideoResult.Status == EnumResultStatus.Done ? 1 : 0,
                                       CountClassForum = x.ClassForumResults.Any() && x.ClassForumResults.All(x => x.Status.HasValue) ? 1 : 0,
                                       CountHomeWork = x.HomeWorkResults.Any() && x.HomeWorkResults.All(x => x.Status == EnumResultStatus.Done) ? 1 : 0
                                   }).ToListAsync();
                    counts.Add(query.Sum(x => x.CountVideo + x.CountClassForum + x.CountHomeWork));
                }
                totalModules.Add(lessonIds.Count * NumberModuleLesson);
            }
            if (mockTestIds != null && mockTestIds.Any())
            {
                if (unitResult != null)
                {
                    var mockTestResults = await _mockTestResultRepository.Queryable.Where(x => x.CourseId == unitResult.CourseId && mockTestIds.Contains(x.MockTestId) && x.UnitId == unitResult.UnitId)
                    .Where(x => x.StudentId == unitResult.StudentId && x.Status == EnumResultStatus.Done).ToListAsync();
                    counts.Add(mockTestResults.Count);
                }
                totalModules.Add(mockTestIds.Count);
            }
            return (counts.Sum(), totalModules.Sum());
        }

        public async Task<(int, int)> GetUnitCompletes(IList<Guid> unitIds, CourseResultModel courseResult)
        {
            var units = await _unitRepository.Queryable.Include(x => x.UnitSkillMockTests).Include(x => x.UnitLessons).Where(x => unitIds.Contains(x.Id)).ToListAsync();
            var lessonIds = units.SelectMany(x => x.UnitLessons).Select(x => x.LessonId).ToList();
            var mockTestIds = units.SelectMany(x => x.UnitSkillMockTests.Select(x => x.MockTestId)).ToList();

            var counts = new List<int>();
            var totalModules = new List<int>();
            if (lessonIds != null && lessonIds.Any())
            {
                var query = await _lessonResultRepository.Queryable
                                  .Where(x => lessonIds.Contains(x.LessonId) && x.CourseId == courseResult.CourseId && x.StudentId == courseResult.StudentId)
                                  .AsNoTracking()
                                  .Select(x => new
                                  {
                                      CountVideo = x.VideoResult != null && x.VideoResult.Status == EnumResultStatus.Done ? 1 : 0,
                                      CountClassForum = x.ClassForumResults.Any() && x.ClassForumResults.All(x => x.Status.HasValue) ? 1 : 0,
                                      CountHomeWork = x.HomeWorkResults.Any() && x.HomeWorkResults.All(x => x.Status == EnumResultStatus.Done) ? 1 : 0
                                  })
                                  .ToListAsync();

                counts.Add(query.Sum(x => x.CountVideo + x.CountClassForum + x.CountHomeWork));
                totalModules.Add(lessonIds.Count * NumberModuleLesson);
            }
            if (mockTestIds != null && mockTestIds.Any())
            {
                var countMockTestDone = await _mockTestResultRepository.Queryable.Where(x => x.CourseId == courseResult.CourseId && mockTestIds.Contains(x.MockTestId))
                    .Where(x => x.StudentId == courseResult.StudentId && x.Status == EnumResultStatus.Done).CountAsync();

                counts.Add(countMockTestDone);
                totalModules.Add(mockTestIds.Count);
            }
            return (counts.Sum(), totalModules.Sum());
        }

        public async Task<(int, int)> GetCompleteCourseAsync(CourseResultModel courseResult)
        {
            ArgumentNullException.ThrowIfNull(courseResult);
            var countTests = new List<(int, int)>();
            var course = await _courseRepository.Queryable.Include(x => x.CourseUnitMockTests).FirstOrDefaultAsync(x => x.Id == courseResult.CourseId);
            if (course == null)
            {
                return (0, 0);
            }
            var unitIds = course.CourseUnitMockTests.Where(x => x.UnitId.HasValue).Select(x => x.UnitId!.Value).ToList();
            if (unitIds != null && unitIds.Any())
            {
                countTests.Add(await GetUnitCompletes(unitIds, courseResult));
            }
            if (courseResult.CourseType == EnumCourseType.Academic)
            {
                var finalTestResult = await _finalTestResultRepository.Queryable.FirstOrDefaultAsync(x => x.StudentId == courseResult.StudentId && x.CourseId == courseResult.CourseId);
                countTests.Add((finalTestResult?.Status == EnumResultStatus.Done ? NumberDefaultComplete : NumberDefault, NumberDefaultComplete));
            }
            else
            {
                var mockTestResults = await _mockTestResultRepository.Queryable.Where(x => x.StudentId == courseResult.StudentId && !x.UnitId.HasValue && x.CourseId == courseResult.CourseId).ToListAsync();
                countTests.Add((mockTestResults.Where(x => x.Status == EnumResultStatus.Done).Count(), mockTestResults.Count));
            }
            return (countTests.Sum(x => x.Item1), countTests.Sum(x => x.Item2));
        }
    }
}
