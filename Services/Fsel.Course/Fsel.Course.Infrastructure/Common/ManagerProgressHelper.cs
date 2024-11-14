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
        private readonly ICourseUnitMockTestRepository _courseUnitMockTestRepository;
        private const int NumberModuleLesson = 3;
        private const int NumberDefaultComplete = 1;
        private const int NumberDefault = 0;

        public ManagerProgressHelper(ILessonResultRepository lessonResultRepository,
            IUnitResultRepository unitResultRepository,
            IFinalTestResultRepository finalTestResultRepository,
            ICourseRepository courseRepository,
            ICourseResultRepository courseResultRepository,
            IUnitRepository unitRepository,
            IMockTestResultRepository mockTestResultRepository,
            ICourseUnitMockTestRepository courseUnitMockTestRepository)
        {
            _lessonResultRepository = lessonResultRepository;
            _unitResultRepository = unitResultRepository;
            _finalTestResultRepository = finalTestResultRepository;
            _courseRepository = courseRepository;
            _courseResultRepository = courseResultRepository;
            _unitRepository = unitRepository;
            _mockTestResultRepository = mockTestResultRepository;
            _courseUnitMockTestRepository = courseUnitMockTestRepository;
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

        public async Task<(int, int)> GetUnitCompletes(IList<Guid> unitIds, CourseResultModel courseResult, DateTime? arrivalDate = default)
        {
            var units = await _unitRepository.Queryable.Include(x => x.UnitSkillMockTests).Include(x => x.UnitLessons).Where(x => unitIds.Contains(x.Id)).ToListAsync();
            var lessonIds = units.SelectMany(x => x.UnitLessons).Select(x => x.LessonId).ToList();
            var mockTestIds = units.SelectMany(x => x.UnitSkillMockTests.Select(x => x.MockTestId)).ToList();

            var counts = new List<int>();
            var totalModules = new List<int> { lessonIds.Count * NumberModuleLesson, mockTestIds.Count };
            if (lessonIds != null && lessonIds.Any())
            {
                var queryData = await _lessonResultRepository.Queryable
                                  .Where(x => lessonIds.Contains(x.LessonId) && x.CourseId == courseResult.CourseId && x.StudentId == courseResult.StudentId)
                                  .Where(x => !arrivalDate.HasValue || (x.UpdatedDate ?? x.CreatedDate).Date <= arrivalDate.Value.Date)
                                  .Where(x => unitIds.Contains(x.UnitId))
                                  .AsNoTracking()
                                  .GroupBy(x => x.StudentId)
                                  .Select(x => new
                                  {
                                      CountVideo = x.Select(x => x.VideoResult).Where(x => x != null)
                                                    .Where(x => !arrivalDate.HasValue || (x!.UpdatedDate ?? x.CreatedDate).Date <= arrivalDate.Value.Date)
                                                    .Where(x => x!.Status == EnumResultStatus.Done).Count(),
                                      CountClassForum = x.SelectMany(x => x.ClassForumResults)
                                                    .Where(x => !arrivalDate.HasValue || (x!.UpdatedDate ?? x.CreatedDate).Date <= arrivalDate.Value.Date)
                                                    .Where(x => x!.Status.HasValue).Count(),
                                      CountHomeWork = x.SelectMany(x => x.HomeWorkResults).GroupBy(x => x.LessonResultId)
                                                    .Select(x => x.Select(x => x).All(x => x.Status == EnumResultStatus.Done) && x.Select(x => x).All(x => !arrivalDate.HasValue || (x!.UpdatedDate ?? x.CreatedDate).Date <= arrivalDate.Value.Date))
                                                    .Count(),
                                  })
                                  .FirstOrDefaultAsync();
                if (queryData != null)
                {
                    counts.Add(queryData.CountVideo + queryData.CountClassForum + queryData.CountHomeWork);
                }
            }
            if (mockTestIds != null && mockTestIds.Any())
            {
                var countMockTestDone = await _mockTestResultRepository.Queryable.Where(x => x.CourseId == courseResult.CourseId && mockTestIds.Contains(x.MockTestId))
                    .Where(x => !arrivalDate.HasValue || (x.UpdatedDate ?? x.CreatedDate).Date <= arrivalDate.Value.Date)
                    .Where(x => x.UnitId.HasValue && unitIds.Contains(x.UnitId.Value))
                    .Where(x => x.StudentId == courseResult.StudentId && x.Status == EnumResultStatus.Done)
                    .CountAsync();

                counts.Add(countMockTestDone);
            }
            return (counts.Sum(), totalModules.Sum());
        }

        private class OverallModuleLearnModel
        {
            public Guid CourseId { get; set; }
            public int Count { get; set; }
        }

        public async Task<int> GetOverallCompleteAsync(IList<CourseResultModel>? courseResults, DateTime? arrivalDate = default)
        {
            if (courseResults == null)
            {
                return default;
            }
            var groups = await GetCourseCompletesAsync(courseResults, arrivalDate);
            return (int)(groups.Any() ? NumberHelper.ConvertRound(groups.Sum(x => x.CountComplete) / courseResults.Count) : NumberDefault);
        }

        public async Task<int> GetTotalCompleteCourseAsync(IList<CourseResultModel>? courseResults)
        {
            if (courseResults == null)
            {
                return default;
            }
            var courseGroups = await GetCompleteCourseTotalsAsync(courseResults);
            return (int)(courseGroups.Any() ? NumberHelper.ConvertRound(courseGroups.Average(x => x.Count)) : NumberDefault);
        }

        public async Task<(int, int)> GetCompleteCourseAsync(CourseResultModel courseResult, DateTime? arrivalDate = default)
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
                countTests.Add(await GetUnitCompletes(unitIds, courseResult, arrivalDate));
            }
            if (course.CourseType == EnumCourseType.Academic)
            {
                var countDoneFinalTest = await _finalTestResultRepository.Queryable.Where(x => !arrivalDate.HasValue || (x.UpdatedDate ?? x.CreatedDate).Date <= arrivalDate.Value.Date)
                                                                                .Where(x => x.StudentId == courseResult.StudentId && x.CourseId == courseResult.CourseId)
                                                                                .Where(x => x.Status == EnumResultStatus.Done)
                                                                                .CountAsync();

                countTests.Add((countDoneFinalTest, NumberDefaultComplete));
            }
            else
            {
                var mockTestIds = course.CourseUnitMockTests.Where(x => x.MockTestId.HasValue).Select(x => x.MockTestId.GetValueOrDefault()).ToList();
                var countDoneMockTest = await _mockTestResultRepository.Queryable
                    .Where(x => !arrivalDate.HasValue || (x.UpdatedDate ?? x.CreatedDate).Date <= arrivalDate.Value.Date)
                    .Where(x => x.StudentId == courseResult.StudentId && !x.UnitId.HasValue && x.CourseId == courseResult.CourseId)
                    .Where(x => x.Status == EnumResultStatus.Done)
                    .CountAsync();
                countTests.Add((countDoneMockTest, mockTestIds.Count));
            }
            return (countTests.Sum(x => x.Item1), countTests.Sum(x => x.Item2));
        }

        public async Task<IList<CourseCompleteModel>> GetProgressCompleteModuleAsync(IList<CourseResultModel> courseResults, DateTime? arrivalDate = default)
        {
            if (courseResults == null)
            {
                return new List<CourseCompleteModel>();
            }
            var courseCompleteModules = await GetCourseCompletesAsync(courseResults, arrivalDate);
            var courseCompleteTotalModules = await GetCompleteCourseTotalsAsync(courseResults);
            return courseResults.Select(item =>
            {
                var courseCompleteModule = courseCompleteModules.FirstOrDefault(x => x.StudentId == item.StudentId && x.CourseId == item.CourseId) ?? new CourseCompleteModel
                {
                    StudentId = item.StudentId,
                    CourseId = item.CourseId,
                };
                var courseCompleteTotalModule = courseCompleteTotalModules.FirstOrDefault(x => x.CourseId == item.CourseId);
                courseCompleteModule.TotalComplete = courseCompleteTotalModule?.Count ?? default;
                return courseCompleteModule;
            }).ToList();
        }

        private async Task<List<OverallModuleLearnModel>> GetCompleteCourseTotalsAsync(IList<CourseResultModel>? courseResults)
        {
            if (courseResults == null)
            {
                return new List<OverallModuleLearnModel>();
            }
            var courseIds = courseResults.Select(x => x.CourseId).Distinct().ToList();
            var courses = await _courseUnitMockTestRepository.Queryable.Where(x => courseIds.Contains(x.CourseId))
                                                                      .GroupBy(x => x.CourseId)
                                                                      .Select(x => new
                                                                      {
                                                                          CourseId = x.Key,
                                                                          CountLesson = x.Where(x => x.UnitId.HasValue).Select(x => x.Unit).SelectMany(x => x.UnitLessons).Count(),
                                                                          CountSkillMockTest = x.Where(x => x.UnitId.HasValue).Select(x => x.Unit).SelectMany(x => x.UnitSkillMockTests).Count(),
                                                                          CountMockTest = x.Where(x => x.MockTestId.HasValue).Count(),
                                                                          CountFinalTest = x.Where(x => x.FinalTestId.HasValue).Count(),
                                                                      }).ToListAsync();
            return courseResults.Join(courses,
                                      courseResult => courseResult.CourseId,
                                      course => course.CourseId,
                                      (courseResult, course) => course).Select(x => new OverallModuleLearnModel
                                      {
                                          CourseId = x.CourseId,
                                          Count = x.CountLesson * NumberModuleLesson + x.CountFinalTest + x.CountSkillMockTest + x.CountFinalTest
                                      }).ToList();
        }

        public async Task<IList<CourseCompleteModel>> GetCourseCompletesAsync(IList<CourseResultModel> courseResults, DateTime? arrivalDate = default)
        {
            var courseIds = courseResults.Select(x => x.CourseId).ToList();
            var studentIds = courseResults.Select(x => x.StudentId).ToList();
            var dataStudent = courseResults.Select(x => new { StudentId = x.StudentId, CourseId = x.CourseId }).ToList();

            var lessonGroupResults = await _lessonResultRepository.Queryable
                                 .Where(x => courseIds.Contains(x.CourseId) && studentIds.Contains(x.StudentId))
                                 .Where(x => !arrivalDate.HasValue || (x.UpdatedDate ?? x.CreatedDate).Date <= arrivalDate.Value.Date)
                                 .AsNoTracking()
                                 .GroupBy(x => new { x.StudentId, x.CourseId })
                                 .Select(x => new
                                 {
                                     CourseId = x.Key.CourseId,
                                     StudentId = x.Key.StudentId,
                                     CountVideo = x.Select(x => x.VideoResult).Where(x => x != null)
                                                   .Where(x => !arrivalDate.HasValue || (x!.UpdatedDate ?? x.CreatedDate).Date <= arrivalDate.Value.Date)
                                                   .Where(x => x!.Status == EnumResultStatus.Done).Count(),
                                     CountClassForum = x.SelectMany(x => x.ClassForumResults)
                                                   .Where(x => !arrivalDate.HasValue || (x!.UpdatedDate ?? x.CreatedDate).Date <= arrivalDate.Value.Date)
                                                   .Where(x => x!.Status.HasValue).Count(),
                                     CountHomeWork = x.SelectMany(x => x.HomeWorkResults).GroupBy(x => x.LessonResultId)
                                                   .Select(x => x.Select(x => x).All(x => x.Status == EnumResultStatus.Done) && x.Select(x => x).All(x => !arrivalDate.HasValue || (x!.UpdatedDate ?? x.CreatedDate).Date <= arrivalDate.Value.Date))
                                                   .Count(),
                                 }).ToListAsync();

            var mockTestGroupResults = await _mockTestResultRepository.Queryable.Where(x => courseIds.Contains(x.CourseId) && studentIds.Contains(x.StudentId))
                                  .Where(x => !arrivalDate.HasValue || (x.UpdatedDate ?? x.CreatedDate).Date <= arrivalDate.Value.Date)
                                  .Where(x => x.Status == EnumResultStatus.Done)
                                  .GroupBy(x => new { x.StudentId, x.CourseId })
                                  .Select(x => new CourseCompleteModel
                                  {
                                      StudentId = x.Key.StudentId,
                                      CourseId = x.Key.CourseId,
                                      CountComplete = x.Count(),
                                  }).ToListAsync();

            var finalTestGroupResults = await _finalTestResultRepository.Queryable.Where(x => courseIds.Contains(x.CourseId) && studentIds.Contains(x.StudentId))
                                 .Where(x => !arrivalDate.HasValue || (x.UpdatedDate ?? x.CreatedDate).Date <= arrivalDate.Value.Date)
                                 .Where(x => x.Status == EnumResultStatus.Done)
                                 .GroupBy(x => new { x.StudentId, x.CourseId })
                                 .Select(x => new CourseCompleteModel
                                 {
                                     StudentId = x.Key.StudentId,
                                     CourseId = x.Key.CourseId,
                                     CountComplete = x.Count(),
                                 })
                                 .ToListAsync();
            var finalTestJoinResults = finalTestGroupResults.Join(dataStudent,
                                                 finalTestResult => new { finalTestResult.CourseId, finalTestResult.StudentId },
                                                 student => new { student.CourseId, student.StudentId },
                                                 (finalTestResult, student) => finalTestResult)
                                                .Select(x => new CourseCompleteModel
                                                {
                                                    StudentId = x.StudentId,
                                                    CourseId = x.CourseId,
                                                    CountComplete = x.CountComplete
                                                }).ToList();

            var mockTestJoinResults = mockTestGroupResults.Join(dataStudent,
                                             mockTestResult => new { mockTestResult.CourseId, mockTestResult.StudentId },
                                             student => new { student.CourseId, student.StudentId },
                                             (mockTestResult, student) => mockTestResult)
                                            .Select(x => new CourseCompleteModel
                                            {
                                                StudentId = x.StudentId,
                                                CourseId = x.CourseId,
                                                CountComplete = x.CountComplete
                                            }).ToList();
            var lessonJoinResults = lessonGroupResults.Join(dataStudent,
                                            lessonResult => new { lessonResult.CourseId, lessonResult.StudentId },
                                            student => new { student.CourseId, student.StudentId },
                                            (lessonResult, student) => lessonResult)
                                           .Select(x => new CourseCompleteModel
                                           {
                                               StudentId = x.StudentId,
                                               CourseId = x.CourseId,
                                               CountComplete = x.CountClassForum + x.CountVideo + x.CountHomeWork
                                           }).ToList();
            return finalTestJoinResults.Concat(lessonJoinResults).Concat(mockTestJoinResults)
                                            .GroupBy(x => new { x.StudentId, x.CourseId })
                                            .Select(x => new CourseCompleteModel
                                            {
                                                StudentId = x.Key.StudentId,
                                                CourseId = x.Key.CourseId,
                                                CountComplete = x.Sum(x => x.CountComplete)
                                            })
                                            .ToList();
        }
    }
}
