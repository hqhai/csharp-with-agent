// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Common
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Core.Extensions;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Domain.Models.EntityModels.ManagerReportModels;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Helpers;
    using Microsoft.Data.SqlClient;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.DependencyInjection;
    using static Fsel.Shared.Constants.ValueSettings;

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
        private readonly IMapper _mapper;
        private readonly IServiceProvider _serviceProvider;
        private readonly CourseDbContext _courseDbContext;
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
            IMockTestResultRepository mockTestResultRepository,
            ICourseUnitMockTestRepository courseUnitMockTestRepository,
            IMapper mapper,
            IServiceProvider serviceProvider,
            CourseDbContext courseDbContext)
        {
            _lessonResultRepository = lessonResultRepository;
            _unitResultRepository = unitResultRepository;
            _finalTestResultRepository = finalTestResultRepository;
            _courseRepository = courseRepository;
            _courseResultRepository = courseResultRepository;
            _unitRepository = unitRepository;
            _mockTestResultRepository = mockTestResultRepository;
            _courseUnitMockTestRepository = courseUnitMockTestRepository;
            _mapper = mapper;
            _serviceProvider = serviceProvider;
            _courseDbContext = courseDbContext;
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
                ContentCompleted = $"{currentProgress} / {progress}",
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
                    var query = await _lessonResultRepository.Queryable
                                   .Where(x => x.UnitId == unitResult.UnitId && x.CourseId == unitResult.CourseId && x.StudentId == unitResult.StudentId)
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
                                  .Where(x => x.CourseId == courseResult.CourseId && x.StudentId == courseResult.StudentId)
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

        #region

        public async Task<double> GetOverallCompleteAsync(IList<CourseResultModel>? courseResults, DateTime? arrivalDate = default)
        {
            var studentIds = courseResults != null && courseResults.Any() ? courseResults.Select(x => x.StudentId).ToList() : new List<Guid>();
            StringBuilder sb = new StringBuilder();
            foreach (var studentId in studentIds)
            {
                sb.Append(studentId).Append(",");
            }
            // Xóa dấu phẩy cuối cùng
            if (sb.Length > 0)
            {
                sb.Length--;
            }

            object studentIdsParam = sb.Length > 0 ? sb.ToString() : (object)DBNull.Value;

            var endDate = arrivalDate ?? (object)DBNull.Value;
            var courseReports = await _courseDbContext.Set<CourseCompleteReportModel>()
                                   .FromSqlRaw("EXEC ManagerCourseCountComplete @StudentIds, @EndDate",
                                        new SqlParameter("@StudentIds", studentIdsParam),
                                        new SqlParameter("@EndDate", endDate))
                                   .AsNoTracking().ToListAsync();
            if (courseReports == null || !courseReports.Any())
            {
                return default;
            }

            return NumberHelper.ConvertRound(courseReports.Sum(x => x.CountComplete) / courseReports.Count);
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
            if (course.CourseType == EnumCourseType.Academic || course.CourseType == EnumCourseType.EnglishFoundation)
            {
                var finalTestId = course.CourseUnitMockTests.Where(x => x.FinalTestId.HasValue).Select(x => x.FinalTestId.GetValueOrDefault()).FirstOrDefault();
                var countDoneFinalTest = await _finalTestResultRepository.Queryable.Where(x => x.StudentId == courseResult.StudentId && x.CourseId == courseResult.CourseId)
                                                                                .Where(x => x.Status == EnumResultStatus.Done && x.FinalTestId == finalTestId)
                                                                                .CountAsync();

                countTests.Add((countDoneFinalTest, NumberDefaultComplete));
            }
            else
            {
                var mockTestIds = course.CourseUnitMockTests.Where(x => x.MockTestId.HasValue).Select(x => x.MockTestId.GetValueOrDefault()).ToList();
                var countDoneMockTest = await _mockTestResultRepository.Queryable
                    .Where(x => x.StudentId == courseResult.StudentId && !x.UnitId.HasValue && x.CourseId == courseResult.CourseId)
                    .Where(x => x.Status == EnumResultStatus.Done && mockTestIds.Contains(x.MockTestId))
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

            var learnCourseCompletes = new List<CourseCompleteModel>();
            var courseCompleteModules = await GetCourseCompletesAsync(courseResults, arrivalDate: arrivalDate);
            var courseCompleteTotalModules = await GetCompleteCourseTotalsAsync(courseResults);
            courseResults.ForEach(courseResult =>
            {
                var courseCompleteModule = courseCompleteModules.FirstOrDefault(x => x.StudentId == courseResult.StudentId && x.CourseId == courseResult.CourseId) ?? new CourseCompleteModel
                {
                    StudentId = courseResult.StudentId,
                    CourseId = courseResult.CourseId,
                };
                var courseCompleteTotalModule = courseCompleteTotalModules.FirstOrDefault(x => x.CourseId == courseResult.CourseId);
                courseCompleteModule.CourseName = courseCompleteTotalModule?.CourseName;
                courseCompleteModule.TotalComplete = courseCompleteTotalModule?.Count ?? default;
                courseCompleteModule.UnitDisplayOrder = courseCompleteModule.UnitDisplayOrder != 0 ? courseCompleteModule.UnitDisplayOrder : ModuleDefault;
                courseCompleteModule.LessonDisplayOrder = courseCompleteModule.LessonDisplayOrder != 0 ? courseCompleteModule.LessonDisplayOrder : ModuleDefault;
                learnCourseCompletes.Add(courseCompleteModule);
            });
            return learnCourseCompletes;
        }

        public async Task<IList<CourseCompleteModel>> GetCourseCompletesAsync(IList<CourseResultModel>? courseResults, BaseQueryModel? baseQuery = default, DateTime? arrivalDate = default)
        {
            if (courseResults == null || !courseResults.Any())
            {
                return new List<CourseCompleteModel>();
            }
            #region
            //var courseCompletes = new List<CourseCompleteModel>();
            //using (var scope = _serviceProvider.CreateScope())
            //{
            //    var courseResultRepository = scope.ServiceProvider.GetRequiredService<ICourseResultRepository>();
            //    var courseUnitMockTestRepository = scope.ServiceProvider.GetRequiredService<ICourseUnitMockTestRepository>();
            //    var unitResultRepository = scope.ServiceProvider.GetRequiredService<IUnitResultRepository>();
            //    var mockTestResultRepository = scope.ServiceProvider.GetRequiredService<IMockTestResultRepository>();
            //    var finalTestResultRepository = scope.ServiceProvider.GetRequiredService<IFinalTestResultRepository>();
            //    var lessonResultRepository = scope.ServiceProvider.GetRequiredService<ILessonResultRepository>();
            //    var videoResultRepository = scope.ServiceProvider.GetRequiredService<IVideoResultRepository>();
            //    var classForumResultRepository = scope.ServiceProvider.GetRequiredService<IClassForumResultRepository>();

            //    var query = await (from baseQ in courseResultRepository.Queryable.WhereBulkContains(courseResults.Select(x => x.StudentId), x => x.StudentId)

            //                       join cum in courseUnitMockTestRepository.Queryable.AsNoTracking()
            //                       on baseQ.CourseId equals cum.CourseId

            //                       join ur in unitResultRepository.Queryable.AsNoTracking()
            //                       on new { baseQ.StudentId, baseQ.CourseId, UnitId = cum.UnitId } equals new { ur.StudentId, ur.CourseId, UnitId = (Guid?)ur.UnitId } into unitGroup
            //                       from ur in unitGroup.DefaultIfEmpty()

            //                       join skmt in mockTestResultRepository.Queryable.AsNoTracking() on new { ur.StudentId, UnitId = (Guid?)ur.UnitId, ur.CourseId } equals new { skmt.StudentId, UnitId = skmt.UnitId, skmt.CourseId } into skmtGroup
            //                       from skmt in skmtGroup.DefaultIfEmpty()

            //                       join ftr in finalTestResultRepository.Queryable.AsNoTracking()
            //                       on new { baseQ.StudentId, baseQ.CourseId, FinalTestId = cum.FinalTestId } equals new { ftr.StudentId, ftr.CourseId, FinalTestId = (Guid?)ftr.FinalTestId } into ftrGroup
            //                       from ftr in ftrGroup.DefaultIfEmpty()

            //                       join mtr in mockTestResultRepository.Queryable.AsNoTracking()
            //                       on new { baseQ.StudentId, baseQ.CourseId, MockTestId = cum.MockTestId } equals new { mtr.StudentId, mtr.CourseId, MockTestId = (Guid?)mtr.MockTestId } into mtrGroup
            //                       from mtr in mtrGroup.DefaultIfEmpty()

            //                       join lr in lessonResultRepository.Queryable.AsNoTracking()
            //                       on new { ur.StudentId, ur.UnitId, ur.CourseId } equals new { lr.StudentId, lr.UnitId, lr.CourseId } into lrGroup
            //                       from lr in lrGroup.DefaultIfEmpty()

            //                       join vr in videoResultRepository.Queryable.AsNoTracking()
            //                       on lr.Id equals vr.LessonResultId into vrGroup
            //                       from vr in vrGroup.DefaultIfEmpty()

            //                       join clr in classForumResultRepository.Queryable.AsNoTracking()
            //                       on lr.Id equals clr.LessonResultId into clrGroup
            //                       from clr in clrGroup.DefaultIfEmpty()

            //                       where baseQ.WorkingStatus == EnumWorkingStatus.Active
            //                       group new { baseQ, ur, lr, vr, clr, mtr, ftr, skmt }
            //                       by new { baseQ.CourseId, baseQ.StudentId }
            //                               into g
            //                       select new CourseCompleteModel
            //                       {
            //                           StudentId = g.Key.StudentId,
            //                           CourseId = g.Key.CourseId,
            //                           CountComplete = g.Where(x => x.vr.Status == EnumResultStatus.Done)
            //                                            .Where(x => !arrivalDate.HasValue || (x.vr.UpdatedDate ?? x.vr.CreatedDate).Date <= arrivalDate.Value.Date)
            //                                            .Select(x => x.vr.Id).Distinct().Count() +

            //                                            g.Where(x => x.clr.Status.HasValue)
            //                                            .Where(x => !arrivalDate.HasValue || (x.clr.UpdatedDate ?? x.clr.CreatedDate).Date <= arrivalDate.Value.Date)
            //                                            .Select(x => x.clr.Id).Distinct().Count() +

            //                                            g.Where(x => x.lr.Status == EnumResultStatus.Done)
            //                                            .Where(x => !arrivalDate.HasValue || (x.lr.UpdatedDate ?? x.lr.CreatedDate).Date <= arrivalDate.Value.Date)
            //                                            .Select(x => x.lr.Id).Distinct().Count() +

            //                                            g.Where(x => x.ftr.Status == EnumResultStatus.Done)
            //                                             .Where(x => !arrivalDate.HasValue || (x.ftr.UpdatedDate ?? x.ftr.CreatedDate).Date <= arrivalDate.Value.Date)
            //                                             .Select(x => x.ftr.Id).Distinct().Count() +

            //                                            g.Where(x => x.mtr.Status == EnumResultStatus.Done)
            //                                             .Where(x => !arrivalDate.HasValue || (x.mtr.UpdatedDate ?? x.mtr.CreatedDate).Date <= arrivalDate.Value.Date)
            //                                             .Select(x => x.mtr.Id).Distinct().Count() +

            //                                            g.Where(x => x.skmt.Status == EnumResultStatus.Done)
            //                                             .Where(x => !arrivalDate.HasValue || (x.skmt.UpdatedDate ?? x.skmt.CreatedDate).Date <= arrivalDate.Value.Date)
            //                                             .Select(x => x.skmt.Id).Distinct().Count(),

            //                           UnitDisplayOrder = g.Select(x => x.ur).Where(x => x.Status != EnumResultStatus.Unfinished)
            //                                               .Where(x => !arrivalDate.HasValue || (x.CompletionDate ?? x.UpdatedDate ?? x.CreatedDate).Date <= arrivalDate.Value.Date)
            //                                               .OrderBy(x => x.Status == EnumResultStatus.Process ? ValueOrderIndex.OrderIndexProcess :
            //                                                             x.Status == EnumResultStatus.New ? ValueOrderIndex.OrderIndexNew :
            //                                                             x.Status == EnumResultStatus.Done ? ValueOrderIndex.OrderIndexDone : ValueOrderIndex.OrderIndexOther)
            //                                               .ThenByDescending(x => x.UpdatedDate ?? x.CreatedDate).Where(x => x.Unit != null)
            //                                               .Select(x => x.Unit!.CourseUnitMockTests.Where(n => n.CourseId == x.CourseId)
            //                                               .Select(n => n.Number).FirstOrDefault()).FirstOrDefault(),

            //                           LessonDisplayOrder = g.Select(x => x.lr).Where(x => x.Status != EnumResultStatus.Unfinished)
            //                                               .Where(x => !arrivalDate.HasValue || (x.UpdatedDate ?? x.CreatedDate).Date <= arrivalDate.Value.Date)
            //                                               .OrderBy(x => x.Status == EnumResultStatus.Process ? ValueOrderIndex.OrderIndexProcess :
            //                                                             x.Status == EnumResultStatus.New ? ValueOrderIndex.OrderIndexNew :
            //                                                             x.Status == EnumResultStatus.Done ? ValueOrderIndex.OrderIndexDone : ValueOrderIndex.OrderIndexOther)
            //                                               .ThenByDescending(x => x.UpdatedDate ?? x.CreatedDate).Where(x => x.Lesson != null)
            //                                               .Select(x => x.Lesson!.UnitLessons.Where(n => n.UnitId == x.UnitId).Select(n => n.DisplayOrder).FirstOrDefault()).FirstOrDefault()
            //                       }).ToListAsync();
            //    courseCompletes = baseQuery != null && baseQuery.SortBy.Any() ? query.ApplySortAndPaging(baseQuery).ToList() : query.ApplySort(baseQuery).ToList();
            //}
            //return courseCompletes;
            #endregion
            var studentIds = courseResults.Select(x => x.StudentId).ToList();
            StringBuilder sb = new StringBuilder();
            foreach (var studentId in studentIds)
            {
                sb.Append(studentId).Append(",");
            }
            // Xóa dấu phẩy cuối cùng
            if (sb.Length > 0)
            {
                sb.Length--;
            }
            object studentIdsParam = sb.Length > 0 ? sb.ToString() : (object)DBNull.Value;

            var endDate = arrivalDate ?? (object)DBNull.Value;
            var courseReports = await _courseDbContext.Set<CourseCompleteReportModel>()
                                   .FromSqlRaw("EXEC ManagerCourseComplete @StudentIds, @EndDate",
                                        new SqlParameter("@StudentIds", studentIdsParam),
                                        new SqlParameter("@EndDate", endDate))
                                   .AsNoTracking().ToListAsync();

            return _mapper.Map<IList<CourseCompleteModel>>(courseReports);
        }

        public async Task<IList<CourseCompleteModel>> GetCourseCompletesFilterAsync(IList<CourseResultModel>? courseResults, BaseQueryModel? baseQuery = default, DateTime? arrivalDate = default, bool isSearchReport = false)
        {
            if (courseResults == null || !courseResults.Any())
            {
                return new List<CourseCompleteModel>();
            }
            var courseCompletes = new List<CourseCompleteModel>();

            using (var scope = _serviceProvider.CreateScope())
            {
                var courseResultRepository = scope.ServiceProvider.GetRequiredService<ICourseResultRepository>();
                var courseUnitMockTestRepository = scope.ServiceProvider.GetRequiredService<ICourseUnitMockTestRepository>();
                var unitResultRepository = scope.ServiceProvider.GetRequiredService<IUnitResultRepository>();

                var query = from baseQ in courseResultRepository.Queryable.WhereBulkContains(courseResults.Select(x => x.StudentId), x => x.StudentId)

                            join cum in courseUnitMockTestRepository.Queryable on baseQ.CourseId equals cum.CourseId

                            join ur in unitResultRepository.Queryable
                            on new { baseQ.StudentId, baseQ.CourseId, UnitId = cum.UnitId } equals new { ur.StudentId, ur.CourseId, UnitId = (Guid?)ur.UnitId }

                            where baseQ.WorkingStatus == EnumWorkingStatus.Active && ur.Status != EnumResultStatus.Unfinished
                            group new { ur }
                            by new { ur.CourseId, ur.StudentId } into g
                            select new CourseCompleteModel
                            {
                                StudentId = g.Key.StudentId,
                                CourseId = g.Key.CourseId,
                                UnitDisplayOrder = g.Select(x => x.ur).Where(x => !arrivalDate.HasValue || (x.CompletionDate ?? x.UpdatedDate ?? x.CreatedDate).Date <= arrivalDate.Value.Date)
                                                    .OrderBy(x => x.Status == EnumResultStatus.Process ? ValueOrderIndex.OrderIndexProcess :
                                                                  x.Status == EnumResultStatus.New ? ValueOrderIndex.OrderIndexNew :
                                                                  x.Status == EnumResultStatus.Done ? ValueOrderIndex.OrderIndexDone : ValueOrderIndex.OrderIndexOther)
                                                    .ThenByDescending(x => x.UpdatedDate ?? x.CreatedDate).Where(x => x.Unit != null)
                                                    .Select(x => x.Unit!.CourseUnitMockTests.Where(n => n.CourseId == x.CourseId)
                                                    .Select(n => n.Number).FirstOrDefault()).FirstOrDefault(),
                            };
                courseCompletes = baseQuery != null && baseQuery.SortBy.Any() && isSearchReport ? await query.ApplySortAndPaging(baseQuery).ToListAsync() : await query.ApplySort(baseQuery).ToListAsync();
            }
            return courseCompletes;
        }

        public async Task<IList<CourseCompleteModel>> GetCourseCompletesFilterCountCompleteAsync(IList<CourseResultModel>? courseResults, BaseQueryModel? baseQuery = default, DateTime? arrivalDate = default, bool isSearchReport = false)
        {
            if (courseResults == null || !courseResults.Any())
            {
                return new List<CourseCompleteModel>();
            }
            var courseCompletes = new List<CourseCompleteModel>();

            using (var scope = _serviceProvider.CreateScope())
            {
                var courseResultRepository = scope.ServiceProvider.GetRequiredService<ICourseResultRepository>();
                var courseUnitMockTestRepository = scope.ServiceProvider.GetRequiredService<ICourseUnitMockTestRepository>();
                var unitResultRepository = scope.ServiceProvider.GetRequiredService<IUnitResultRepository>();
                var mockTestResultRepository = scope.ServiceProvider.GetRequiredService<IMockTestResultRepository>();
                var finalTestResultRepository = scope.ServiceProvider.GetRequiredService<IFinalTestResultRepository>();
                var lessonResultRepository = scope.ServiceProvider.GetRequiredService<ILessonResultRepository>();
                var videoResultRepository = scope.ServiceProvider.GetRequiredService<IVideoResultRepository>();
                var classForumResultRepository = scope.ServiceProvider.GetRequiredService<IClassForumResultRepository>();

                var query = from baseQ in courseResultRepository.Queryable.WhereBulkContains(courseResults.Select(x => x.StudentId), x => x.StudentId)

                            join cum in courseUnitMockTestRepository.Queryable
                            on baseQ.CourseId equals cum.CourseId
                            join ur in unitResultRepository.Queryable
                                                       on new { baseQ.StudentId, baseQ.CourseId, UnitId = cum.UnitId } equals new { ur.StudentId, ur.CourseId, UnitId = (Guid?)ur.UnitId } into unitGroup
                            from ur in unitGroup.DefaultIfEmpty()

                            join skmt in mockTestResultRepository.Queryable on new { ur.StudentId, UnitId = (Guid?)ur.UnitId, ur.CourseId } equals new { skmt.StudentId, UnitId = skmt.UnitId, skmt.CourseId } into skmtGroup
                            from skmt in skmtGroup.DefaultIfEmpty()

                            join ftr in finalTestResultRepository.Queryable
                            on new { baseQ.StudentId, baseQ.CourseId, FinalTestId = cum.FinalTestId } equals new { ftr.StudentId, ftr.CourseId, FinalTestId = (Guid?)ftr.FinalTestId } into ftrGroup
                            from ftr in ftrGroup.DefaultIfEmpty()

                            join mtr in mockTestResultRepository.Queryable
                            on new { baseQ.StudentId, baseQ.CourseId, MockTestId = cum.MockTestId } equals new { mtr.StudentId, mtr.CourseId, MockTestId = (Guid?)mtr.MockTestId } into mtrGroup
                            from mtr in mtrGroup.DefaultIfEmpty()

                            join lr in lessonResultRepository.Queryable
                            on new { ur.StudentId, ur.UnitId, ur.CourseId } equals new { lr.StudentId, lr.UnitId, lr.CourseId } into lrGroup
                            from lr in lrGroup.DefaultIfEmpty()

                            join vr in videoResultRepository.Queryable
                            on lr.Id equals vr.LessonResultId into vrGroup
                            from vr in vrGroup.DefaultIfEmpty()

                            join clr in classForumResultRepository.Queryable
                            on lr.Id equals clr.LessonResultId into clrGroup
                            from clr in clrGroup.DefaultIfEmpty()

                            where baseQ.WorkingStatus == EnumWorkingStatus.Active
                            group new { baseQ, ur, lr, vr, clr, mtr, ftr, skmt }
                            by new { baseQ.CourseId, baseQ.StudentId } into g
                            select new CourseCompleteModel
                            {
                                StudentId = g.Key.StudentId,
                                CourseId = g.Key.CourseId,
                                CountComplete = g.Where(x => x.vr.Status == EnumResultStatus.Done)
                                                 .Where(x => !arrivalDate.HasValue || (x.vr.UpdatedDate ?? x.vr.CreatedDate).Date <= arrivalDate.Value.Date)
                                                 .Select(x => x.vr.Id).Distinct().Count() +

                                                 g.Where(x => x.clr.Status.HasValue)
                                                 .Where(x => !arrivalDate.HasValue || (x.clr.UpdatedDate ?? x.clr.CreatedDate).Date <= arrivalDate.Value.Date)
                                                 .Select(x => x.clr.Id).Distinct().Count() +

                                                 g.Where(x => x.lr.Status == EnumResultStatus.Done)
                                                 .Where(x => !arrivalDate.HasValue || (x.lr.UpdatedDate ?? x.lr.CreatedDate).Date <= arrivalDate.Value.Date)
                                                 .Select(x => x.lr.Id).Distinct().Count() +

                                                 g.Where(x => x.ftr.Status == EnumResultStatus.Done)
                                                  .Where(x => !arrivalDate.HasValue || (x.ftr.UpdatedDate ?? x.ftr.CreatedDate).Date <= arrivalDate.Value.Date)
                                                  .Select(x => x.ftr.Id).Distinct().Count() +

                                                 g.Where(x => x.mtr.Status == EnumResultStatus.Done)
                                                  .Where(x => !arrivalDate.HasValue || (x.mtr.UpdatedDate ?? x.mtr.CreatedDate).Date <= arrivalDate.Value.Date)
                                                  .Select(x => x.mtr.Id).Distinct().Count() +

                                                 g.Where(x => x.skmt.Status == EnumResultStatus.Done)
                                                  .Where(x => !arrivalDate.HasValue || (x.skmt.UpdatedDate ?? x.skmt.CreatedDate).Date <= arrivalDate.Value.Date)
                                                  .Select(x => x.skmt.Id).Distinct().Count(),
                            };
                courseCompletes = baseQuery != null && baseQuery.SortBy.Any() && isSearchReport ? await query.ApplySortAndPaging(baseQuery).ToListAsync() : await query.ApplySort(baseQuery).ToListAsync();
            }
            return courseCompletes;
        }

        public async Task<IList<CourseCompleteModel>> GetCourseLearnsAsync(IList<CourseResultModel>? courseResults)
        {
            if (courseResults == null || !courseResults.Any())
            {
                return new List<CourseCompleteModel>();
            }

            var query = from baseQ in _courseResultRepository.Queryable.WhereBulkContains(courseResults.Select(x => x.StudentId), x => x.StudentId)
                        where baseQ.WorkingStatus == EnumWorkingStatus.Active
                        select new CourseCompleteModel
                        {
                            StudentId = baseQ.StudentId,
                            CourseId = baseQ.CourseId,
                        };
            return await query.ToListAsync();
        }

        private async Task<List<OverallModuleLearnModel>> GetCompleteCourseTotalsAsync(IList<CourseResultModel>? courseResults)
        {
            if (courseResults == null || !courseResults.Any())
            {
                return new List<OverallModuleLearnModel>();
            }
            var courseIds = courseResults.Select(x => x.CourseId).Distinct().ToList();
            var courses = await _courseUnitMockTestRepository.Queryable.WhereBulkContains(courseIds, x => x.CourseId)
                                                                       .GroupBy(x => x.CourseId)
                                                                       .Select(x => new
                                                                       {
                                                                           CourseId = x.Key,
                                                                           CourseName = x.Select(x => x.Course!.Name).FirstOrDefault(),
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
                                          CourseName = x.CourseName,
                                          Count = x.CountLesson * NumberModuleLesson + x.CountMockTest + x.CountSkillMockTest + x.CountFinalTest
                                      }).ToList();
        }

        public async Task<IList<CourseCompleteModel>> GetCourseCompleteToExportsAsync(IList<CourseResultModel>? courseResults)
        {
            if (courseResults == null || !courseResults.Any())
            {
                return new List<CourseCompleteModel>();
            }
            var learnCourseCompletes = new List<CourseCompleteModel>();
            using (var scope = _serviceProvider.CreateScope())
            {
                var courseResultRepository = scope.ServiceProvider.GetRequiredService<ICourseResultRepository>();
                var courseUnitMockTestRepository = scope.ServiceProvider.GetRequiredService<ICourseUnitMockTestRepository>();
                var unitResultRepository = scope.ServiceProvider.GetRequiredService<IUnitResultRepository>();
                var mockTestResultRepository = scope.ServiceProvider.GetRequiredService<IMockTestResultRepository>();
                var finalTestResultRepository = scope.ServiceProvider.GetRequiredService<IFinalTestResultRepository>();
                var lessonResultRepository = scope.ServiceProvider.GetRequiredService<ILessonResultRepository>();
                var videoResultRepository = scope.ServiceProvider.GetRequiredService<IVideoResultRepository>();
                var classForumResultRepository = scope.ServiceProvider.GetRequiredService<IClassForumResultRepository>();
                var unitLessonRepository = scope.ServiceProvider.GetRequiredService<IUnitLessonRepository>();

                var studentCourseKeys = await courseResultRepository.Queryable.Where(x => x.WorkingStatus == EnumWorkingStatus.Active)
                                                                              .WhereBulkContains(courseResults.Select(x => x.StudentId), x => x.StudentId)
                                                                              .Select(x => new { x.StudentId, x.CourseId })
                                                                              .ToListAsync();

                var lessonResults = await lessonResultRepository.Queryable.WhereBulkContains(studentCourseKeys, x => new { x.StudentId, x.CourseId }).ToListAsync();

                var unitLessonLookup = await unitLessonRepository.Queryable
                                  .WhereBulkContains(lessonResults.Select(x => new { x.LessonId, x.UnitId }), ul => new { ul.LessonId, ul.UnitId }) // hoặc filter cụ thể hơn
                                  .Distinct()
                                  .ToListAsync();

                var unitLessonMaps = unitLessonLookup.ToDictionary(g => new { g.UnitId, g.LessonId }, g => g.DisplayOrder);

                var mockTestResults = await mockTestResultRepository.Queryable.Where(x => x.Status == EnumResultStatus.Done)
                                                                              .WhereBulkContains(studentCourseKeys, x => new { x.StudentId, x.CourseId })
                                                                              .ToListAsync();
                var finalTestResults = await finalTestResultRepository.Queryable.Where(x => x.Status == EnumResultStatus.Done)
                                                                                .WhereBulkContains(studentCourseKeys, x => new { x.StudentId, x.CourseId })
                                                                                .ToListAsync();
                var classForumResults = await classForumResultRepository.Queryable.Where(x => x.Status.HasValue)
                                                                                  .WhereBulkContains(lessonResults.Select(x => x.Id), x => x.LessonResultId)
                                                                                  .ToListAsync();
                var videoResults = await videoResultRepository.Queryable.Where(x => x.Status == EnumResultStatus.Done)
                                                                        .WhereBulkContains(lessonResults.Select(x => x.Id), x => x.LessonResultId)
                                                                        .ToListAsync();

                var lessonsLookup = lessonResults
                                   .GroupBy(l => new { l.StudentId, l.CourseId })
                                   .ToDictionary(g => g.Key, g => g.ToList());

                var videosLookup = videoResults
                    .GroupBy(v => v.LessonResultId)
                    .ToDictionary(g => g.Key, g => g.ToList());

                var forumsLookup = classForumResults
                    .GroupBy(c => c.LessonResultId)
                    .ToDictionary(g => g.Key, g => g.ToList());

                var mockTestLookup = mockTestResults
                    .GroupBy(m => new { m.StudentId, m.CourseId })
                    .ToDictionary(g => g.Key, g => g.ToList());

                var finalTestLookup = finalTestResults
                    .GroupBy(f => new { f.StudentId, f.CourseId })
                    .ToDictionary(g => g.Key, g => g.ToList());

                var dataLearn = studentCourseKeys
                    .Select(key =>
                    {
                        lessonsLookup.TryGetValue(key, out var lessons);
                        lessons ??= new List<LessonResult>();

                        var lessonIds = lessons.Select(l => l.Id).ToHashSet();

                        var videoCount = lessonIds.SelectMany(id => videosLookup.TryGetValue(id, out var vids) ? vids : Enumerable.Empty<VideoResult>())
                                                  .Count();

                        var forumCount = lessonIds.SelectMany(id => forumsLookup.TryGetValue(id, out var forums) ? forums : Enumerable.Empty<ClassForumResult>())
                                                  .Count();

                        var doneLessonCount = lessons.Count(l => l.Status == EnumResultStatus.Done);

                        mockTestLookup.TryGetValue(key, out var mockTests);
                        finalTestLookup.TryGetValue(key, out var finalTests);
                        var lessonResult = lessons.Where(x => x.Status != EnumResultStatus.Unfinished)
                                                  .OrderBy(x => x.Status == EnumResultStatus.Process ? ValueOrderIndex.OrderIndexProcess :
                                                                x.Status == EnumResultStatus.New ? ValueOrderIndex.OrderIndexNew :
                                                                x.Status == EnumResultStatus.Done ? ValueOrderIndex.OrderIndexDone : ValueOrderIndex.OrderIndexOther)
                                                  .ThenByDescending(x => x.UpdatedDate ?? x.CreatedDate).FirstOrDefault();

                        int? displayOrder = null;
                        if (lessonResult != null && unitLessonMaps.TryGetValue(new { lessonResult.UnitId, lessonResult.LessonId }, out var order))
                        {
                            displayOrder = order;
                        }

                        return new CourseCompleteModel
                        {
                            StudentId = key.StudentId,
                            CourseId = key.CourseId,
                            TotalLessonDone = doneLessonCount,
                            LessonDisplayOrder = displayOrder,
                            LessonResult = _mapper.Map<LessonResultModel>(lessonResult),
                            CountComplete = videoCount + forumCount + doneLessonCount
                                            + (mockTests?.Count ?? 0)
                                            + (finalTests?.Count ?? 0)
                        };
                    }).ToList();

                var queryUnit = from baseQ in courseResultRepository.Queryable.Where(x => x.WorkingStatus == EnumWorkingStatus.Active).WhereBulkContains(courseResults.Select(x => x.StudentId), x => x.StudentId)

                                join cum in courseUnitMockTestRepository.Queryable
                                on baseQ.CourseId equals cum.CourseId

                                join ur in unitResultRepository.Queryable
                                on new { baseQ.StudentId, baseQ.CourseId, UnitId = cum.UnitId } equals new { ur.StudentId, ur.CourseId, UnitId = (Guid?)ur.UnitId }

                                group new { baseQ, ur }
                                by new { baseQ.CourseId, baseQ.StudentId } into g
                                select new CourseCompleteModel
                                {
                                    StudentId = g.Key.StudentId,
                                    CourseId = g.Key.CourseId,
                                    UnitResult = g.Select(x => x.ur).Where(x => x.Status != EnumResultStatus.Unfinished)
                                                  .OrderBy(x => x.Status == EnumResultStatus.Process ? ValueOrderIndex.OrderIndexProcess :
                                                                x.Status == EnumResultStatus.New ? ValueOrderIndex.OrderIndexNew :
                                                                x.Status == EnumResultStatus.Done ? ValueOrderIndex.OrderIndexDone : ValueOrderIndex.OrderIndexOther)
                                                  .ThenByDescending(x => x.UpdatedDate ?? x.CreatedDate).Select(x => _mapper.Map<UnitResultModel>(x)).FirstOrDefault(),

                                    UnitDisplayOrder = g.Select(x => x.ur).Where(x => x.Status != EnumResultStatus.Unfinished)
                                                  .OrderBy(x => x.Status == EnumResultStatus.Process ? ValueOrderIndex.OrderIndexProcess :
                                                                x.Status == EnumResultStatus.New ? ValueOrderIndex.OrderIndexNew :
                                                                x.Status == EnumResultStatus.Done ? ValueOrderIndex.OrderIndexDone : ValueOrderIndex.OrderIndexOther)
                                                  .ThenByDescending(x => x.UpdatedDate ?? x.CreatedDate).Where(x => x.Unit != null)
                                                  .Select(x => x.Unit!.CourseUnitMockTests.Where(n => n.CourseId == x.CourseId)
                                                  .Select(n => n.Number).FirstOrDefault()).FirstOrDefault(),
                                };
                var unitData = await queryUnit.AsNoTracking().ToListAsync();

                learnCourseCompletes = (from cc in dataLearn
                                        join ls in unitData
                                            on new { cc.StudentId, cc.CourseId } equals new { ls.StudentId, ls.CourseId } into joinGroup
                                        from ls in joinGroup.DefaultIfEmpty()
                                        select new CourseCompleteModel
                                        {
                                            StudentId = cc.StudentId,
                                            CourseId = cc.CourseId,
                                            CountComplete = cc.CountComplete,
                                            TotalLessonDone = cc?.TotalLessonDone ?? 0,
                                            LessonResult = cc?.LessonResult,
                                            UnitResult = ls?.UnitResult,
                                            LessonDisplayOrder = cc.LessonDisplayOrder,
                                            UnitDisplayOrder = ls?.UnitDisplayOrder
                                        }).ToList();
            }
            return learnCourseCompletes;
        }

        public async Task<IList<CourseCompleteModel>> GetProgressCompleteModuleExportAsync(IList<CourseResultModel> courseResults)
        {
            if (courseResults == null || !courseResults.Any())
            {
                return new List<CourseCompleteModel>();
            }
            var learnCourseCompletes = new List<CourseCompleteModel>();
            var courseCompleteModules = await GetCourseCompleteToExportsAsync(courseResults);
            var courseCompleteTotalModules = await GetCompleteCourseTotalsAsync(courseResults);
            courseResults.ForEach(courseResult =>
            {
                var courseCompleteModule = courseCompleteModules.FirstOrDefault(x => x.StudentId == courseResult.StudentId) ?? new CourseCompleteModel
                {
                    StudentId = courseResult.StudentId,
                    CourseId = courseResult.CourseId,
                };
                var courseCompleteTotalModule = courseCompleteTotalModules.FirstOrDefault(x => x.CourseId == courseCompleteModule.CourseId);
                if (courseCompleteTotalModule != null)
                {
                    courseCompleteModule.TotalComplete = courseCompleteTotalModule.Count;
                }
                courseCompleteModule.UnitDisplayOrder = courseCompleteModule.UnitDisplayOrder != 0 ? courseCompleteModule.UnitDisplayOrder : ModuleDefault;
                courseCompleteModule.LessonDisplayOrder = courseCompleteModule.LessonDisplayOrder != 0 ? courseCompleteModule.LessonDisplayOrder : ModuleDefault;
                learnCourseCompletes.Add(courseCompleteModule);
            });
            return learnCourseCompletes.ToList();
        }

        #endregion

        public async Task<IList<CourseCompleteModel>> GetProgressCompleteLessonAsync(IList<CourseResultModel> courseResults, DateTime? arrivalDate = default)
        {
            if (courseResults == null || !courseResults.Any())
            {
                return new List<CourseCompleteModel>();
            }

            var learnCourseCompletes = new List<CourseCompleteModel>();
            var courseCompleteModules = await GetCourseCompleteLessonsAsync(courseResults, arrivalDate: arrivalDate);
            var courseCompleteTotalModules = await GetCourseTotalLessonsAsync(courseResults);
            var completeModuleDict = courseCompleteModules.ToDictionary(x => (x.StudentId, x.CourseId));
            var totalLessonDict = courseCompleteTotalModules.ToDictionary(x => x.CourseId);

            courseResults.ForEach(courseResult =>
            {
                completeModuleDict.TryGetValue((courseResult.StudentId, courseResult.CourseId), out var courseCompleteModule);
                totalLessonDict.TryGetValue(courseResult.CourseId, out var courseCompleteTotalModule);
                courseCompleteModule ??= new CourseCompleteModel
                {
                    StudentId = courseResult.StudentId,
                    CourseId = courseResult.CourseId,
                };

                courseCompleteModule.TotalLesson = courseCompleteTotalModule?.TotalLesson ?? default;
                courseCompleteModule.UnitDisplayOrder = courseCompleteModule.UnitDisplayOrder != 0 ? courseCompleteModule.UnitDisplayOrder : ModuleDefault;
                courseCompleteModule.LessonDisplayOrder = courseCompleteModule.LessonDisplayOrder != 0 ? courseCompleteModule.LessonDisplayOrder : ModuleDefault;
                learnCourseCompletes.Add(courseCompleteModule);
            });
            return learnCourseCompletes;
        }

        public async Task<IList<CourseCompleteModel>> GetCourseCompleteLessonsAsync(IList<CourseResultModel>? courseResults, BaseQueryModel? baseQuery = default, DateTime? arrivalDate = default)
        {
            if (courseResults == null || !courseResults.Any())
            {
                return new List<CourseCompleteModel>();
            }

            var studentIds = courseResults.Select(x => x.StudentId).Distinct().ToList();

            using var scope = _serviceProvider.CreateScope();
            var courseResultRepository = scope.ServiceProvider.GetRequiredService<ICourseResultRepository>();
            var courseUnitMockTestRepository = scope.ServiceProvider.GetRequiredService<ICourseUnitMockTestRepository>();
            var unitResultRepository = scope.ServiceProvider.GetRequiredService<IUnitResultRepository>();
            var lessonResultRepository = scope.ServiceProvider.GetRequiredService<ILessonResultRepository>();

            // Lọc trước các dữ liệu cần thiết
            var baseQs = await courseResultRepository.Queryable
                .WhereBulkContains(studentIds, x => x.StudentId)
                .Where(x => x.WorkingStatus == EnumWorkingStatus.Active)
                .Select(x => new { x.StudentId, x.CourseId })
                .Distinct()
                .ToListAsync();

            var unitResults = await unitResultRepository.Queryable.WhereBulkContains(baseQs, new[] { "StudentId", "CourseId" })
                .Where(x => !arrivalDate.HasValue || (x.CompletionDate ?? x.UpdatedDate ?? x.CreatedDate).Date <= arrivalDate.Value.Date)
                .Select(x => new
                {
                    x.StudentId,
                    x.CourseId,
                    x.UnitId,
                    x.Status,
                    x.UpdatedDate,
                    x.CreatedDate,
                    x.CompletionDate,
                    DisplayOrder = x.Unit.CourseUnitMockTests
                        .Where(c => c.CourseId == x.CourseId)
                        .Select(c => c.Number).FirstOrDefault()
                })
                .ToListAsync();

            var lessonResults = await lessonResultRepository.Queryable.WhereBulkContains(baseQs, new[] { "StudentId", "CourseId" })
                .Where(x => !arrivalDate.HasValue || (x.UpdatedDate ?? x.CreatedDate).Date <= arrivalDate.Value.Date)
                .Select(x => new
                {
                    x.StudentId,
                    x.CourseId,
                    x.UnitId,
                    x.Status,
                    x.UpdatedDate,
                    x.CreatedDate,
                    x.Id,
                    DisplayOrder = x.Lesson.UnitLessons
                        .Where(u => u.UnitId == x.UnitId)
                        .Select(u => u.DisplayOrder).FirstOrDefault()
                })
                .ToListAsync();

            // Gộp dữ liệu lại thành CourseCompleteModel
            var result = baseQs.Select(q =>
            {
                var studentLessonResults = lessonResults
                    .Where(x => x.StudentId == q.StudentId && x.CourseId == q.CourseId)
                    .ToList();

                var studentUnitResults = unitResults
                    .Where(x => x.StudentId == q.StudentId && x.CourseId == q.CourseId)
                    .ToList();

                return new CourseCompleteModel
                {
                    StudentId = q.StudentId,
                    CourseId = q.CourseId,
                    TotalLessonDone = studentLessonResults
                        .Where(x => x.Status == EnumResultStatus.Done)
                        .Select(x => x.Id).Distinct().Count(),

                    UnitDisplayOrder = studentUnitResults
                        .Where(x => x.Status != EnumResultStatus.Unfinished)
                        .OrderBy(x => x.Status == EnumResultStatus.Process ? ValueOrderIndex.OrderIndexProcess :
                                      x.Status == EnumResultStatus.New ? ValueOrderIndex.OrderIndexNew :
                                      x.Status == EnumResultStatus.Done ? ValueOrderIndex.OrderIndexDone :
                                      ValueOrderIndex.OrderIndexOther)
                        .ThenByDescending(x => x.UpdatedDate ?? x.CreatedDate)
                        .Select(x => x.DisplayOrder).FirstOrDefault(),

                    LessonDisplayOrder = studentLessonResults
                        .Where(x => x.Status != EnumResultStatus.Unfinished)
                        .OrderBy(x => x.Status == EnumResultStatus.Process ? ValueOrderIndex.OrderIndexProcess :
                                      x.Status == EnumResultStatus.New ? ValueOrderIndex.OrderIndexNew :
                                      x.Status == EnumResultStatus.Done ? ValueOrderIndex.OrderIndexDone :
                                      ValueOrderIndex.OrderIndexOther)
                        .ThenByDescending(x => x.UpdatedDate ?? x.CreatedDate)
                        .Select(x => x.DisplayOrder).FirstOrDefault()
                };
            }).ToList();

            return baseQuery != null && baseQuery.SortBy.Any()
                ? result.AsQueryable().ApplySortAndPaging(baseQuery).ToList()
                : result.AsQueryable().ApplySort(baseQuery).ToList();
        }

        public async Task<IList<CourseCompleteModel>> GetCourseCompletesFilterCountAsync(IList<CourseResultModel>? courseResults, BaseQueryModel? baseQuery = default, DateTime? arrivalDate = default, bool isSearchReport = false)
        {
            if (courseResults == null || !courseResults.Any())
            {
                return new List<CourseCompleteModel>();
            }

            var studentIds = courseResults.Select(x => x.StudentId).Distinct().ToList();

            using var scope = _serviceProvider.CreateScope();
            var courseResultRepository = scope.ServiceProvider.GetRequiredService<ICourseResultRepository>();
            var lessonResultRepository = scope.ServiceProvider.GetRequiredService<ILessonResultRepository>();

            // Lọc các CourseResult đang active
            var activeCourseResults = await courseResultRepository.Queryable
                .WhereBulkContains(studentIds, x => x.StudentId)
                .Where(x => x.WorkingStatus == EnumWorkingStatus.Active)
                .Select(x => new { x.StudentId, x.CourseId })
                .ToListAsync();

            // Lọc LessonResult theo StudentId + CourseId + Status + ArrivalDate
            var lessonResults = await lessonResultRepository.Queryable
                .WhereBulkContains(activeCourseResults, new[] { "StudentId", "CourseId" })
                .Where(x => x.Status == EnumResultStatus.Done)
                .Where(x => !arrivalDate.HasValue || (x.UpdatedDate ?? x.CreatedDate).Date <= arrivalDate.Value.Date)
                .Select(x => new { x.StudentId, x.CourseId, x.Id })
                .ToListAsync();

            // Nhóm và tạo kết quả
            var grouped = lessonResults
                .GroupBy(x => new { x.StudentId, x.CourseId })
                .Select(g => new CourseCompleteModel
                {
                    StudentId = g.Key.StudentId,
                    CourseId = g.Key.CourseId,
                    TotalLessonDone = g.Select(x => x.Id).Distinct().Count()
                })
                .ToList();

            // Sắp xếp & phân trang nếu cần
            return baseQuery != null && baseQuery.SortBy.Any() && isSearchReport
                ? grouped.AsQueryable().ApplySortAndPaging(baseQuery).ToList()
                : grouped.AsQueryable().ApplySort(baseQuery).ToList();
        }

        private async Task<List<CourseLessonModel>> GetCourseTotalLessonsAsync(IList<CourseResultModel>? courseResults)
        {
            if (courseResults == null || !courseResults.Any())
            {
                return new List<CourseLessonModel>();
            }

            var courseIds = courseResults
                .Select(x => x.CourseId)
                .Distinct()
                .ToList();

            using var scope = _serviceProvider.CreateScope();
            var courseUnitMockTestRepository = scope.ServiceProvider.GetRequiredService<ICourseUnitMockTestRepository>();
            var unitLessonRepository = scope.ServiceProvider.GetRequiredService<IUnitLessonRepository>();

            // Lấy các UnitId từ các CourseId cần thiết
            var unitMappings = await courseUnitMockTestRepository.Queryable
                .WhereBulkContains(courseIds, x => x.CourseId)
                .Select(x => new { x.CourseId, x.UnitId })
                .Distinct()
                .ToListAsync();

            var unitIds = unitMappings.Select(x => x.UnitId).Distinct().ToList();

            // Lấy tất cả UnitLesson liên quan
            var unitLessons = await unitLessonRepository.Queryable
                .WhereBulkContains(unitIds, x => x.UnitId)
                .Select(x => new { x.Id, x.UnitId })
                .ToListAsync();

            // Ghép và đếm theo CourseId
            var courseLessons = unitMappings
                .Join(unitLessons, m => m.UnitId, l => l.UnitId, (m, l) => new { m.CourseId, l.Id })
                .GroupBy(x => x.CourseId)
                .Select(g => new CourseLessonModel
                {
                    CourseId = g.Key,
                    TotalLesson = g.Count()
                })
                .ToList();

            return courseLessons;
        }
    }
}
