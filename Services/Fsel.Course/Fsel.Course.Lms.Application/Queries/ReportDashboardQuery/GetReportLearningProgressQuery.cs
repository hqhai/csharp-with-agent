// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.ReportDashboardQuery
{
    using System.Linq;
    using System.Text;
    using System.Text.Json.Serialization;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Helpers;
    using Fsel.Core.Base;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels.BaseChartModels;
    using Fsel.Course.Domain.Models.EntityModels.ManagerReportModels;
    using Fsel.Course.Infrastructure;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Helpers;
    using MediatR;
    using Microsoft.Data.SqlClient;
    using Microsoft.EntityFrameworkCore;
    using static Fsel.Shared.Constants.ValueSettings;
    using Unit = Domain.Entities.Unit;

    public class GetReportLearningProgressQuery : IRequest<MethodResult<DashBoardLearningProgressModel>>
    {
        public DateTime? EndDate { get; set; }
        public string? SchoolClassStr { get; set; }
        public string? EnumCourseTypeStr { get; set; }
        public string? EnumCourseLevelStr { get; set; }
        public string? Type { get; set; } = nameof(Unit);
        public int DisplayOrderUnit { get; set; } = 1;

        [JsonIgnore]
        public IList<EnumCourseType>? CourseTypes
        {
            get
            {
                return EnumCourseTypeStr.ToList<EnumCourseType>();
            }
        }

        [JsonIgnore]
        public IList<EnumCourseLevel>? CourseLevels
        {
            get
            {
                return EnumCourseLevelStr.ToList<EnumCourseLevel>();
            }
        }
    }

    public class GetReportLearningProgressQueryHandler : IRequestHandler<GetReportLearningProgressQuery, MethodResult<DashBoardLearningProgressModel>>
    {
        private readonly IUserService _userService;
        private readonly IUnitResultRepository _unitResultRepository;
        private readonly ICourseResultRepository _courseResultRepository;
        private readonly ICourseRepository _courseRepository;
        private readonly ICourseUnitMockTestRepository _courseUnitMockTestRepository;
        private readonly AuthContext _authContext;
        private readonly ILessonResultRepository _lessonResultRepository;
        private readonly IUnitLessonRepository _unitLessonRepository;
        private readonly CourseDbContext _courseDbContext;

        public GetReportLearningProgressQueryHandler(IUserService userService,
                                                   IUnitResultRepository unitResultRepository,
                                                   ICourseResultRepository courseResultRepository,
                                                   ICourseRepository courseRepository,
                                                   ICourseUnitMockTestRepository courseUnitMockTestRepository,
                                                   AuthContext authContext,
                                                   ILessonResultRepository lessonResultRepository,
                                                   IUnitLessonRepository unitLessonRepository,
                                                   CourseDbContext courseDbContext)
        {
            _userService = userService;
            _unitResultRepository = unitResultRepository;
            _courseResultRepository = courseResultRepository;
            _courseRepository = courseRepository;
            _courseUnitMockTestRepository = courseUnitMockTestRepository;
            _authContext = authContext;
            _lessonResultRepository = lessonResultRepository;
            _unitLessonRepository = unitLessonRepository;
            _courseDbContext = courseDbContext;
        }

        public async Task<MethodResult<DashBoardLearningProgressModel>> Handle(GetReportLearningProgressQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<DashBoardLearningProgressModel>();

            var courseLevels = request.CourseTypes.GetCourseLevels(request.CourseLevels);

            //var userResults = await _userService.GetStudentsDashboardAsync(new GetStudentsDashboardQueryModel
            //{
            //    EnumCourseLevelStr = string.Join(",", courseLevels),
            //    SchoolClassStr = request.SchoolClassStr,
            //});
            //if (!userResults.IsSuccessStatusCode)
            //{
            //    methodResult.AddError(userResults.Error);
            //    return methodResult;
            //}
            //var studentIds = userResults.Content?.Result?.Select(x => x.Id).ToList() ?? new List<Guid>();

            if (request.CourseLevels != null && request.CourseLevels.Any())
            {
                courseLevels = request.CourseLevels.Intersect(courseLevels).ToList();
            }
            var schoolIdResult = await _userService.GetSchoolIdAsync();
            var schoolId = schoolIdResult.Content?.Result ?? Guid.NewGuid();
            object? courseLevelsParam = null;
            if (courseLevels != null && courseLevels.Any())
            {
                courseLevelsParam = new StringBuilder().AppendJoin(",", courseLevels).ToString();
            }
            else
            {
                courseLevelsParam = DBNull.Value;
            }
            var schoolClassParam = request.SchoolClassStr != null ? request.SchoolClassStr : (object)DBNull.Value;
            var endDate = request.EndDate ?? (object)DBNull.Value;

            var queryPieChart = await _courseDbContext.Set<ReportLearningProcessModel>()
                                   .FromSqlRaw("EXEC DashBoardStudentProgress @CourseLevels, @SchoolClasses, @SchoolId , @EndDate",
                                        new SqlParameter("@CourseLevels", courseLevelsParam),
                                        new SqlParameter("@SchoolClasses", schoolClassParam),
                                        new SqlParameter("@SchoolId", schoolId),
                                        new SqlParameter("@EndDate", endDate))
                                   .AsNoTracking().ToListAsync(cancellationToken);

            // bool isMaxHoursCompleted = true;
            //var queryPieChart = await (from baseQ in _courseResultRepository.Queryable.WhereBulkContains(studentIds, x => x.StudentId)
            //                           join c in _courseRepository.Queryable on baseQ.CourseId equals c.Id
            //                           join lr in _lessonResultRepository.Queryable on new { baseQ.StudentId, baseQ.CourseId } equals new { lr.StudentId, lr.CourseId }
            //                           where baseQ.WorkingStatus == EnumWorkingStatus.Active &&
            //                           (!request.EndDate.HasValue || (lr.UpdatedDate ?? lr.CreatedDate).Date <= request.EndDate.Value.Date) &&
            //                           ((courseLevels == null || !courseLevels.Any()) || courseLevels.Contains(c.CourseLevel)) &&
            //                           lr.Status == EnumResultStatus.Done
            //                           group new { lr } by new { baseQ.CourseId, baseQ.StudentId } into g
            //                           select g.OrderByDescending(x => x.lr.UpdatedDate).ThenByDescending(x => x.lr.CreatedDate)
            //                           .Select(x => isMaxHoursCompleted).FirstOrDefault()).ToListAsync(cancellationToken);
            //.Select(x => !x.lr.MaxHoursCompleted.HasValue || ((x.lr.CompletionDate ?? x.lr.UpdatedDate) - x.lr.NewDate).GetValueOrDefault().TotalSeconds <= x.lr.MaxHoursCompleted).FirstOrDefault();

            var totalStudents = queryPieChart.Count(x => x.IsMaxHoursCompleted);
            var totalBehindSchedule = queryPieChart.Count(x => !x.IsMaxHoursCompleted);
            var totalOnSchedule = totalStudents - totalBehindSchedule;

            var reportLearningProgress = new DashBoardLearningProgressModel
            {
                TotalStudentBehindSchedule = totalBehindSchedule,
                TotalStudentOnSchedule = totalOnSchedule,
                LearningProgressChart = new LearningProgressChartModel
                {
                    Type = EnumChartType.PieChart,
                    LearningProgressDatas = totalStudents > 0
                        ? new List<DataPieChartModel>
                        {
                            new DataPieChartModel
                            {
                                Label = nameof(EnumLearningProgress.OnSchedule),
                                Percent = (int)NumberHelper.GetPercent(totalOnSchedule, totalStudents),
                                Value = totalOnSchedule
                            },
                            new DataPieChartModel
                            {
                                Label = nameof(EnumLearningProgress.BehindSchedule),
                                Percent = (int)NumberHelper.GetPercent(totalBehindSchedule, totalStudents),
                                Value = totalBehindSchedule
                            }
                        } : ConvertHelper.EnumToList<EnumLearningProgress>()
                                      .Select(x => new DataPieChartModel { Label = x.ToString() })
                                      .ToList()
                }
            };
            if (request.Type == nameof(Unit))
            {
                var unitOverallsDict = await _courseDbContext.Set<ReportLearningProcessModel>()
                       .FromSqlRaw("EXEC DashBoardUnitStudentProgress @CourseLevels , @SchoolClasses, @SchoolId, @EndDate",
                                new SqlParameter("@CourseLevels", courseLevelsParam),
                                new SqlParameter("@SchoolClasses", schoolClassParam),
                                new SqlParameter("@SchoolId", schoolId),
                                new SqlParameter("@EndDate", endDate))
                       .AsNoTracking()
                       .ToListAsync(cancellationToken);
                var dataUnitOverall = unitOverallsDict.ToDictionary(x => x.DisplayOrder, x => x.TotalUnitDone);

                //var unitOverallsDict = await (from baseQ in _courseResultRepository.Queryable.WhereBulkContains(studentIds, x => x.StudentId)
                //                              join c in _courseRepository.Queryable on baseQ.CourseId equals c.Id
                //                              join cum in _courseUnitMockTestRepository.Queryable on c.Id equals cum.CourseId
                //                              join ur in _unitResultRepository.Queryable on new { baseQ.StudentId, baseQ.CourseId, UnitId = cum.UnitId }
                //                                 equals new { ur.StudentId, ur.CourseId, UnitId = (Guid?)ur.UnitId }
                //                              where baseQ.WorkingStatus == EnumWorkingStatus.Active &&
                //                                    ((courseLevels == null || !courseLevels.Any()) || courseLevels.Contains(c.CourseLevel)) &&
                //                                    (!request.EndDate.HasValue || (ur.CompletionDate ?? ur.UpdatedDate ?? ur.CreatedDate).Date <= request.EndDate.Value.Date)
                //                              group ur by cum.Number into g
                //                              select new
                //                              {
                //                                  DisplayOrder = g.Key,
                //                                  TotalUnitDone = g.Where(x => x.Status == EnumResultStatus.Done).Select(x => x.Id).Distinct().Count()
                //                              }).ToDictionaryAsync(x => x.DisplayOrder, x => x.TotalUnitDone, cancellationToken);

                reportLearningProgress.OverallLearningProgress = new BaseChartResultModel
                {
                    Type = EnumChartType.LineChart,
                    DataCharts = Enumerable.Range(1, CourseProgressValue.CountUnitAca).Select(item =>
                        new DataChartModel
                        {
                            Label = $"{item}",
                            Value = dataUnitOverall.TryGetValue(item, out var totalDone) ? totalDone : 0
                        }).ToList()
                };
            }
            if (request.Type == nameof(Lesson))
            {
                var lessonOverallsDict = await _courseDbContext.Set<ReportLearningProcessModel>()
                                                               .FromSqlRaw("EXEC DashBoardLessonStudentProgress @CourseLevels, @SchoolClasses, @SchoolId, @EndDate,@DisplayOrderUnit",
                                                                 new SqlParameter("@CourseLevels", courseLevelsParam),
                                                                 new SqlParameter("@SchoolClasses", schoolClassParam),
                                                                 new SqlParameter("@SchoolId", schoolId),
                                                                 new SqlParameter("@EndDate", endDate),
                                                                 new SqlParameter("@DisplayOrderUnit", request.DisplayOrderUnit))
                                                               .AsNoTracking()
                                                               .ToListAsync(cancellationToken);
                var dataLessonOverall = lessonOverallsDict.ToDictionary(x => x.DisplayOrder, x => x.TotalLessonDone);
                //var lessonOverallsDict = await (from baseQ in _courseResultRepository.Queryable.WhereBulkContains(studentIds, x => x.StudentId)
                //                                join c in _courseRepository.Queryable on baseQ.CourseId equals c.Id
                //                                join cum in _courseUnitMockTestRepository.Queryable on c.Id equals cum.CourseId
                //                                join lr in _lessonResultRepository.Queryable on new { baseQ.StudentId, baseQ.CourseId, UnitId = cum.UnitId }
                //                                    equals new { lr.StudentId, lr.CourseId, UnitId = (Guid?)lr.UnitId }
                //                                join ul in _unitLessonRepository.Queryable on new { lr.UnitId, lr.LessonId }
                //                                    equals new { ul.UnitId, ul.LessonId }

                //                                where baseQ.WorkingStatus == EnumWorkingStatus.Active &&
                //                                      ((courseLevels == null || !courseLevels.Any()) || courseLevels.Contains(c.CourseLevel)) &&
                //                                      (!request.EndDate.HasValue || (lr.UpdatedDate ?? lr.CreatedDate).Date <= request.EndDate.Value.Date) &&
                //                                      cum.DisplayOrder == request.DisplayOrderUnit

                //                                group lr by ul.DisplayOrder into g
                //                                select new
                //                                {
                //                                    DisplayOrder = g.Key,
                //                                    TotalLessonDone = g.Count(x => x.Status == EnumResultStatus.Done)
                //                                }).ToDictionaryAsync(x => x.DisplayOrder, x => x.TotalLessonDone, cancellationToken);

                reportLearningProgress.OverallLearningProgress = new BaseChartResultModel
                {
                    Type = EnumChartType.LineChart,
                    DataCharts = Enumerable.Range(1, CourseProgressValue.CountLessonAca).Select(item =>
                        new DataChartModel
                        {
                            Label = $"{item}",
                            Value = dataLessonOverall.TryGetValue(item, out var totalDone) ? totalDone : 0
                        }).ToList()
                };
            }
            methodResult.Result = reportLearningProgress;
            return methodResult;
        }
    }
}
