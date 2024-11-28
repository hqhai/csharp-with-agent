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
    using static Fsel.Shared.Constants.ValueSettings;

    public class ManagerProgressHelper
    {
        private readonly ILessonResultRepository _lessonResultRepository;
        private readonly IUnitResultRepository _unitResultRepository;
        private readonly IFinalTestResultRepository _finalTestResultRepository;
        private readonly ICourseRepository _courseRepository;
        private readonly ICourseResultRepository _courseResultRepository;
        private readonly IUnitRepository _unitRepository;
        private readonly IVideoResultRepository _videoResultRepository;
        private readonly IHomeWorkResultRepository _homeWorkResultRepository;
        private readonly IClassForumResultRepository _classForumResultRepository;
        private readonly IMockTestResultRepository _mockTestResultRepository;
        private readonly ICourseUnitMockTestRepository _courseUnitMockTestRepository;
        private const int NumberModuleLesson = 3;
        private const int NumberDefaultComplete = 1;
        private const int NumberDefault = 0;
        private const int ModuleDefault = 1;

        public ManagerProgressHelper(ILessonResultRepository lessonResultRepository,
            IUnitResultRepository unitResultRepository,
            IFinalTestResultRepository finalTestResultRepository,
            ICourseRepository courseRepository,
            ICourseResultRepository courseResultRepository,
            IUnitRepository unitRepository,
            IVideoResultRepository videoResultRepository,
            IHomeWorkResultRepository homeWorkResultRepository,
            IClassForumResultRepository classForumResultRepository,
            IMockTestResultRepository mockTestResultRepository,
            ICourseUnitMockTestRepository courseUnitMockTestRepository)
        {
            _lessonResultRepository = lessonResultRepository;
            _unitResultRepository = unitResultRepository;
            _finalTestResultRepository = finalTestResultRepository;
            _courseRepository = courseRepository;
            _courseResultRepository = courseResultRepository;
            _unitRepository = unitRepository;
            _videoResultRepository = videoResultRepository;
            _homeWorkResultRepository = homeWorkResultRepository;
            _classForumResultRepository = classForumResultRepository;
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

        public async Task<double> GetOverallCompleteAsync(IList<CourseResultModel>? courseResults, DateTime? arrivalDate = default)
        {
            if (courseResults == null || !courseResults.Any())
            {
                return default;
            }
            var query = await (from baseQ in _courseResultRepository.Queryable

                               join cum in _courseUnitMockTestRepository.Queryable
                               on baseQ.CourseId equals cum.CourseId

                               join ur in _unitResultRepository.Queryable
                               on new { baseQ.StudentId, baseQ.CourseId, UnitId = cum.UnitId } equals new { ur.StudentId, ur.CourseId, UnitId = (Guid?)ur.UnitId } into unitGroup
                               from ur in unitGroup.DefaultIfEmpty()

                               join skmt in _mockTestResultRepository.Queryable on new { ur.StudentId, UnitId = (Guid?)ur.UnitId, ur.CourseId } equals new { skmt.StudentId, UnitId = skmt.UnitId, skmt.CourseId } into skmtGroup
                               from skmt in skmtGroup.DefaultIfEmpty()

                               join ftr in _finalTestResultRepository.Queryable
                               on new { baseQ.StudentId, baseQ.CourseId, FinalTestId = cum.FinalTestId } equals new { ftr.StudentId, ftr.CourseId, FinalTestId = (Guid?)ftr.FinalTestId } into ftrGroup
                               from ftr in ftrGroup.DefaultIfEmpty()

                               join mtr in _mockTestResultRepository.Queryable
                               on new { baseQ.StudentId, baseQ.CourseId, MockTestId = cum.MockTestId } equals new { mtr.StudentId, mtr.CourseId, MockTestId = (Guid?)mtr.MockTestId } into mtrGroup
                               from mtr in mtrGroup.DefaultIfEmpty()

                               join lr in _lessonResultRepository.Queryable
                               on new { ur.StudentId, ur.UnitId, ur.CourseId } equals new { lr.StudentId, lr.UnitId, lr.CourseId } into lrGroup
                               from lr in lrGroup.DefaultIfEmpty()

                               join vr in _videoResultRepository.Queryable
                               on lr.Id equals vr.LessonResultId into vrGroup
                               from vr in vrGroup.DefaultIfEmpty()

                               join clr in _classForumResultRepository.Queryable
                               on lr.Id equals clr.LessonResultId into clrGroup
                               from clr in clrGroup.DefaultIfEmpty()

                               join hwr in _homeWorkResultRepository.Queryable
                               on lr.Id equals hwr.LessonResultId into hwrGroup
                               from hwr in hwrGroup.DefaultIfEmpty()

                               where courseResults.Select(x => x.StudentId).Contains(baseQ.StudentId)
                               && baseQ.WorkingStatus == EnumWorkingStatus.Active
                               && (!arrivalDate.HasValue ||
                                 ((ftr.UpdatedDate ?? ftr.CreatedDate).Date <= arrivalDate.Value.Date
                               && (mtr.UpdatedDate ?? mtr.CreatedDate).Date <= arrivalDate.Value.Date
                               && (lr.UpdatedDate ?? lr.CreatedDate).Date <= arrivalDate.Value.Date
                               && (vr.UpdatedDate ?? vr.CreatedDate).Date <= arrivalDate.Value.Date
                               && (clr.UpdatedDate ?? clr.CreatedDate).Date <= arrivalDate.Value.Date
                               && (hwr.UpdatedDate ?? hwr.CreatedDate).Date <= arrivalDate.Value.Date
                               && (skmt.UpdatedDate ?? skmt.CreatedDate).Date <= arrivalDate.Value.Date
                               ))
                               group new { baseQ, vr, clr, hwr, mtr, ftr, skmt }
                               by new { baseQ.CourseId, baseQ.StudentId }
                               into g
                               select new
                               {
                                   StudentId = g.Key.StudentId,
                                   CountComplete = g.Where(x => x.vr.Status == EnumResultStatus.Done).Select(x => x.vr.Id).Distinct().Count() +
                                                   g.Where(x => x.clr.Status.HasValue).Select(x => x.clr.Id).Distinct().Count() +
                                                   g.GroupBy(x => x.hwr.LessonResultId)
                                                     .Count(hwrGroup => hwrGroup.All(hw => hw.hwr.Status == EnumResultStatus.Done) &&
                                                                        hwrGroup.All(hw => !arrivalDate.HasValue ||
                                                                                       (hw.hwr.UpdatedDate ?? hw.hwr.CreatedDate).Date <= arrivalDate.Value.Date)) +
                                                   g.Where(x => x.ftr.Status == EnumResultStatus.Done).Select(x => x.ftr.Id).Distinct().Count() +
                                                   g.Where(x => x.mtr.Status == EnumResultStatus.Done).Select(x => x.mtr.Id).Distinct().Count() +
                                                   g.Where(x => x.skmt.Status == EnumResultStatus.Done).Select(x => x.skmt.Id).Distinct().Count(),
                               }).SumAsync(x => x.CountComplete);
            return NumberHelper.ConvertRound(query / courseResults.Count);
        }

        public async Task<int> GetTotalCompleteCourseAsync(IList<CourseResultModel>? courseResults)
        {
            if (courseResults == null || !courseResults.Any())
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
            if (courseResults == null || !courseResults.Any())
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
                courseCompleteModule.UnitDisplayOrder = courseCompleteModule.UnitDisplayOrder != 0 ? courseCompleteModule.UnitDisplayOrder : ModuleDefault;
                courseCompleteModule.LessonDisplayOrder = courseCompleteModule.LessonDisplayOrder != 0 ? courseCompleteModule.LessonDisplayOrder : ModuleDefault;
                return courseCompleteModule;
            }).ToList();
        }

        public async Task<IList<CourseCompleteModel>> GetCourseCompletesAsync(IList<CourseResultModel>? courseResults, DateTime? arrivalDate = default)
        {
            if (courseResults == null || !courseResults.Any())
            {
                return new List<CourseCompleteModel>();
            }
            var courseCompletes = await (from baseQ in _courseResultRepository.Queryable

                                         join cum in _courseUnitMockTestRepository.Queryable
                                         on baseQ.CourseId equals cum.CourseId

                                         join ur in _unitResultRepository.Queryable
                                         on new { baseQ.StudentId, baseQ.CourseId, UnitId = cum.UnitId } equals new { ur.StudentId, ur.CourseId, UnitId = (Guid?)ur.UnitId } into unitGroup
                                         from ur in unitGroup.DefaultIfEmpty()

                                         join skmt in _mockTestResultRepository.Queryable on new { ur.StudentId, UnitId = (Guid?)ur.UnitId, ur.CourseId } equals new { skmt.StudentId, UnitId = skmt.UnitId, skmt.CourseId } into skmtGroup
                                         from skmt in skmtGroup.DefaultIfEmpty()

                                         join ftr in _finalTestResultRepository.Queryable
                                         on new { baseQ.StudentId, baseQ.CourseId, FinalTestId = cum.FinalTestId } equals new { ftr.StudentId, ftr.CourseId, FinalTestId = (Guid?)ftr.FinalTestId } into ftrGroup
                                         from ftr in ftrGroup.DefaultIfEmpty()

                                         join mtr in _mockTestResultRepository.Queryable
                                         on new { baseQ.StudentId, baseQ.CourseId, MockTestId = cum.MockTestId } equals new { mtr.StudentId, mtr.CourseId, MockTestId = (Guid?)mtr.MockTestId } into mtrGroup
                                         from mtr in mtrGroup.DefaultIfEmpty()

                                         join lr in _lessonResultRepository.Queryable
                                         on new { ur.StudentId, ur.UnitId, ur.CourseId } equals new { lr.StudentId, lr.UnitId, lr.CourseId } into lrGroup
                                         from lr in lrGroup.DefaultIfEmpty()

                                         join vr in _videoResultRepository.Queryable
                                         on lr.Id equals vr.LessonResultId into vrGroup
                                         from vr in vrGroup.DefaultIfEmpty()

                                         join clr in _classForumResultRepository.Queryable
                                         on lr.Id equals clr.LessonResultId into clrGroup
                                         from clr in clrGroup.DefaultIfEmpty()

                                         join hwr in _homeWorkResultRepository.Queryable
                                         on lr.Id equals hwr.LessonResultId into hwrGroup
                                         from hwr in hwrGroup.DefaultIfEmpty()

                                         where courseResults.Select(x => x.StudentId).Contains(baseQ.StudentId)
                                         && baseQ.WorkingStatus == EnumWorkingStatus.Active
                                         && (!arrivalDate.HasValue ||
                                           ((ftr.UpdatedDate ?? ftr.CreatedDate).Date <= arrivalDate.Value.Date
                                         && (mtr.UpdatedDate ?? mtr.CreatedDate).Date <= arrivalDate.Value.Date
                                         && (lr.UpdatedDate ?? lr.CreatedDate).Date <= arrivalDate.Value.Date
                                         && (vr.UpdatedDate ?? vr.CreatedDate).Date <= arrivalDate.Value.Date
                                         && (clr.UpdatedDate ?? clr.CreatedDate).Date <= arrivalDate.Value.Date
                                         && (hwr.UpdatedDate ?? hwr.CreatedDate).Date <= arrivalDate.Value.Date
                                         && (skmt.UpdatedDate ?? skmt.CreatedDate).Date <= arrivalDate.Value.Date
                                         ))
                                         group new { baseQ, ur, lr, vr, clr, hwr, mtr, ftr, skmt }
                                         by new { baseQ.CourseId, baseQ.StudentId }
                                         into g
                                         select new CourseCompleteModel
                                         {
                                             StudentId = g.Key.StudentId,
                                             CourseId = g.Key.CourseId,
                                             CountComplete = g.Where(x => x.vr.Status == EnumResultStatus.Done).Select(x => x.vr.Id).Distinct().Count() +
                                                             g.Where(x => x.clr.Status.HasValue).Select(x => x.clr.Id).Distinct().Count() +
                                                             g.GroupBy(x => x.hwr.LessonResultId)
                                                               .Count(hwrGroup => hwrGroup.All(hw => hw.hwr.Status == EnumResultStatus.Done) &&
                                                                                  hwrGroup.All(hw => !arrivalDate.HasValue ||
                                                                                                 (hw.hwr.UpdatedDate ?? hw.hwr.CreatedDate).Date <= arrivalDate.Value.Date)) +
                                                             g.Where(x => x.ftr.Status == EnumResultStatus.Done).Select(x => x.ftr.Id).Distinct().Count() +
                                                             g.Where(x => x.mtr.Status == EnumResultStatus.Done).Select(x => x.mtr.Id).Distinct().Count() +
                                                             g.Where(x => x.skmt.Status == EnumResultStatus.Done).Select(x => x.skmt.Id).Distinct().Count(),
                                             UnitDisplayOrder = g.Select(x => x.ur).Where(x => x.Status != EnumResultStatus.Unfinished)
                                                                 .OrderBy(x => x.Status == EnumResultStatus.Process ? ValueOrderIndex.OrderIndexProcess :
                                                                               x.Status == EnumResultStatus.New ? ValueOrderIndex.OrderIndexNew :
                                                                               x.Status == EnumResultStatus.Done ? ValueOrderIndex.OrderIndexDone : ValueOrderIndex.OrderIndexOther)
                                                                 .ThenByDescending(x => x.UpdatedDate ?? x.CreatedDate).Where(x => x.Unit != null).Select(x => x.Unit!.CourseUnitMockTests.Where(n => n.CourseId == x.CourseId).Select(n => n.Number).FirstOrDefault()).FirstOrDefault(),
                                             LessonDisplayOrder = g.Select(x => x.lr).Where(x => x.Status != EnumResultStatus.Unfinished)
                                                                 .OrderBy(x => x.Status == EnumResultStatus.Process ? ValueOrderIndex.OrderIndexProcess :
                                                                               x.Status == EnumResultStatus.New ? ValueOrderIndex.OrderIndexNew :
                                                                               x.Status == EnumResultStatus.Done ? ValueOrderIndex.OrderIndexDone : ValueOrderIndex.OrderIndexOther)
                                                                 .ThenByDescending(x => x.UpdatedDate ?? x.CreatedDate).Where(x => x.Lesson != null).Select(x => x.Lesson!.UnitLessons.Where(n => n.UnitId == x.UnitId).Select(n => n.DisplayOrder).FirstOrDefault()).FirstOrDefault()
                                         }).ToListAsync();
            return courseCompletes;
        }

        private async Task<List<OverallModuleLearnModel>> GetCompleteCourseTotalsAsync(IList<CourseResultModel>? courseResults)
        {
            if (courseResults == null || !courseResults.Any())
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
    }
}
