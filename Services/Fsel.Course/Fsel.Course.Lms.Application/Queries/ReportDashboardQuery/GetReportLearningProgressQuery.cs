// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.ReportDashboardQuery
{
    using System.Linq;
    using System.Text.Json.Serialization;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Helpers;
    using Fsel.Core.Base;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels.BaseChartModels;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Course.Lms.Application.Services.UserServices.QueryModels;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Helpers;
    using MediatR;
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
        public IList<string>? SchoolClasses
        {
            get
            {
                return SchoolClassStr.ToList<string>();
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

        public GetReportLearningProgressQueryHandler(IUserService userService,
                                                   IUnitResultRepository unitResultRepository,
                                                   ICourseResultRepository courseResultRepository,
                                                   ICourseRepository courseRepository,
                                                   ICourseUnitMockTestRepository courseUnitMockTestRepository,
                                                   AuthContext authContext,
                                                   ILessonResultRepository lessonResultRepository,
                                                   IUnitLessonRepository unitLessonRepository)
        {
            _userService = userService;
            _unitResultRepository = unitResultRepository;
            _courseResultRepository = courseResultRepository;
            _courseRepository = courseRepository;
            _courseUnitMockTestRepository = courseUnitMockTestRepository;
            _authContext = authContext;
            _lessonResultRepository = lessonResultRepository;
            _unitLessonRepository = unitLessonRepository;
        }

        public async Task<MethodResult<DashBoardLearningProgressModel>> Handle(GetReportLearningProgressQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<DashBoardLearningProgressModel>();
            var studentIds = new List<Guid>();
            var courseLevels = request.CourseTypes?.SelectMany(x => EnumCourseLevelHelper.GetEnumCourseLevels(x)).ToList() ?? new List<EnumCourseLevel>();
            bool isRoleAdminSchool = _authContext.Roles != null && _authContext.Roles.Contains(EnumRole.AdminSchool.ToString());
            if (isRoleAdminSchool)
            {
                var userResults = await _userService.GetStudentsDashboardAsync(new GetStudentsDashboardQueryModel
                {
                    CourseLevels = courseLevels,
                    SchoolClasses = request.SchoolClasses,
                });
                if (!userResults.IsSuccessStatusCode)
                {
                    methodResult.AddError(userResults.Error);
                    return methodResult;
                }
                studentIds = userResults.Content?.Result?.Select(x => x.Id).ToList() ?? new List<Guid>();
            }

            if (request.CourseLevels != null && request.CourseLevels.Any())
            {
                courseLevels = request.CourseLevels.Intersect(courseLevels).ToList();
            }
            var query = from baseQ in _courseResultRepository.Queryable
                        join c in _courseRepository.Queryable on baseQ.CourseId equals c.Id
                        join lr in _lessonResultRepository.Queryable on new { baseQ.StudentId, baseQ.CourseId } equals new { lr.StudentId, lr.CourseId }
                        where baseQ.WorkingStatus == EnumWorkingStatus.Active &&
                        (!isRoleAdminSchool || ((studentIds == null || !studentIds.Any()) || studentIds.Contains(baseQ.StudentId))) &&
                        (!request.EndDate.HasValue || (lr.CompletionDate ?? lr.UpdatedDate ?? lr.CreatedDate).Date <= request.EndDate.Value.Date) &&
                        ((courseLevels == null || !courseLevels.Any()) || courseLevels.Contains(c.CourseLevel)) &&
                        lr.Status == EnumResultStatus.Done
                        group new { lr } by new { baseQ.CourseId, baseQ.StudentId } into g
                        select g.OrderByDescending(x => x.lr.CompletionDate).ThenByDescending(x => x.lr.UpdatedDate)
                        .Select(x => !x.lr.MaxHoursCompleted.HasValue || ((x.lr.CompletionDate ?? x.lr.UpdatedDate) - x.lr.NewDate).GetValueOrDefault().TotalSeconds <= x.lr.MaxHoursCompleted).FirstOrDefault();
            var queryPieChart = await query.ToListAsync(cancellationToken);
            var reportLearningProgress = new DashBoardLearningProgressModel
            {
                TotalStudentBehindSchedule = queryPieChart.Count(x => !x),
                TotalStudentOnSchedule = queryPieChart.Count(x => x),
                LearningProgressChart = new LearningProgressChartModel
                {
                    Type = EnumChartType.PieChart,
                    LearningProgressDatas = queryPieChart.Any() ? queryPieChart.GroupBy(x => x).Select(x => new DataPieChartModel
                    {
                        Label = x.Key ? nameof(EnumLearningProgress.OnSchedule) : nameof(EnumLearningProgress.BehindSchedule),
                        Percent = (int)NumberHelper.GetPercent(queryPieChart.Count(y => y == x.Key), queryPieChart.Count),
                        Value = queryPieChart.Count(y => y == x.Key)
                    }).ToList()
                    : ConvertHelper.EnumToList<EnumLearningProgress>().Select(x => new DataPieChartModel { Label = x.ToString() }).ToList()
                }
            };
            if (request.Type == nameof(Unit))
            {
                var unitOveralls = await (from baseQ in _courseResultRepository.Queryable
                                          join c in _courseRepository.Queryable on baseQ.CourseId equals c.Id
                                          join cum in _courseUnitMockTestRepository.Queryable on c.Id equals cum.CourseId
                                          join ur in _unitResultRepository.Queryable on new { baseQ.StudentId, baseQ.CourseId, UnitId = cum.UnitId } equals new { ur.StudentId, ur.CourseId, UnitId = (Guid?)ur.UnitId }
                                          where baseQ.WorkingStatus == EnumWorkingStatus.Active &&
                                          (!isRoleAdminSchool || ((studentIds == null || !studentIds.Any()) || studentIds.Contains(baseQ.StudentId))) &&
                                          ((courseLevels == null || !courseLevels.Any()) || courseLevels.Contains(c.CourseLevel)) &&
                                          (!request.EndDate.HasValue || (ur.CompletionDate ?? ur.UpdatedDate ?? ur.CreatedDate).Date <= request.EndDate.Value.Date)
                                          group new { cum, ur } by new { cum.Number } into g
                                          select new
                                          {
                                              DisplayOrder = g.Key.Number,
                                              TotalUnitDone = g.Select(x => x.ur).Where(x => x.Status == EnumResultStatus.Done).Count(),
                                          }).ToListAsync(cancellationToken);
                reportLearningProgress.OverallLearningProgress = new BaseChartResultModel
                {
                    Type = EnumChartType.LineChart,
                    DataCharts = Enumerable.Range(1, CourseProgressValue.CountUnitAca).Select(item =>
                    {
                        var unitOverall = unitOveralls.FirstOrDefault(x => x.DisplayOrder == item);
                        return new DataChartModel
                        {
                            Label = $"{nameof(Unit)} {item}",
                            Value = unitOverall?.TotalUnitDone ?? default,
                        };
                    }).ToList()
                };
            }
            if (request.Type == nameof(Lesson))
            {
                var lessonOveralls = await (from baseQ in _courseResultRepository.Queryable
                                            join c in _courseRepository.Queryable on baseQ.CourseId equals c.Id
                                            join cum in _courseUnitMockTestRepository.Queryable on c.Id equals cum.CourseId
                                            join ur in _unitResultRepository.Queryable on new { baseQ.StudentId, baseQ.CourseId, UnitId = cum.UnitId } equals new { ur.StudentId, ur.CourseId, UnitId = (Guid?)ur.UnitId }
                                            join lr in _lessonResultRepository.Queryable on new { ur.StudentId, ur.CourseId, ur.UnitId } equals new { lr.StudentId, lr.CourseId, lr.UnitId }
                                            join ul in _unitLessonRepository.Queryable on new { lr.UnitId, lr.LessonId } equals new { ul.UnitId, ul.LessonId }

                                            where baseQ.WorkingStatus == EnumWorkingStatus.Active &&
                                            (!isRoleAdminSchool || ((studentIds == null || !studentIds.Any()) || studentIds.Contains(baseQ.StudentId))) &&
                                            ((courseLevels == null || !courseLevels.Any()) || courseLevels.Contains(c.CourseLevel)) &&
                                            (!request.EndDate.HasValue || (ur.CompletionDate ?? ur.UpdatedDate ?? ur.CreatedDate).Date <= request.EndDate.Value.Date) &&
                                            cum.DisplayOrder == request.DisplayOrderUnit

                                            group lr by ul.DisplayOrder into g
                                            select new
                                            {
                                                DisplayOrder = g.Key,
                                                TotalLessonDone = g.Select(x => x).Where(x => x.Status == EnumResultStatus.Done).Count(),
                                            }).ToListAsync(cancellationToken);

                reportLearningProgress.OverallLearningProgress = new BaseChartResultModel
                {
                    Type = EnumChartType.LineChart,
                    DataCharts = Enumerable.Range(1, CourseProgressValue.CountLessonAca).Select(item =>
                    {
                        var lessonOverall = lessonOveralls.FirstOrDefault(x => x.DisplayOrder == item);
                        return new DataChartModel
                        {
                            Label = $"{nameof(Lesson)} {item}",
                            Value = lessonOverall?.TotalLessonDone ?? default,
                        };
                    }).ToList()
                };
            }
            methodResult.Result = reportLearningProgress;
            return methodResult;
        }
    }
}
