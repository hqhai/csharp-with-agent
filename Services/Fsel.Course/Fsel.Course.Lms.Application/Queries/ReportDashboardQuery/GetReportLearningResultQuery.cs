// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.ReportDashboardQuery
{
    using Fsel.Common.ActionResults;
    using Fsel.Common.Helpers;
    using Fsel.Core.Base;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels.BaseChartModels;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Course.Lms.Application.Services.UserServices.QueryModels;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Helpers;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class GetReportLearningResultQuery : IRequest<MethodResult<DashBoardLearningResultModel>>
    {
        public DateTime? EndDate { get; set; }
    }

    public class GetReportLearningResultQueryHandler : IRequestHandler<GetReportLearningResultQuery, MethodResult<DashBoardLearningResultModel>>
    {
        private readonly IUserService _userService;
        private readonly IUnitResultRepository _unitResultRepository;
        private readonly ICourseResultRepository _courseResultRepository;
        private readonly ICourseRepository _courseRepository;
        private readonly ICourseUnitMockTestRepository _courseUnitMockTestRepository;
        private readonly AuthContext _authContext;

        public GetReportLearningResultQueryHandler(IUserService userService,
                                                   IUnitResultRepository unitResultRepository,
                                                   ICourseResultRepository courseResultRepository,
                                                   ICourseRepository courseRepository,
                                                   ICourseUnitMockTestRepository courseUnitMockTestRepository,
                                                   AuthContext authContext)
        {
            _userService = userService;
            _unitResultRepository = unitResultRepository;
            _courseResultRepository = courseResultRepository;
            _courseRepository = courseRepository;
            _courseUnitMockTestRepository = courseUnitMockTestRepository;
            _authContext = authContext;
        }

        public async Task<MethodResult<DashBoardLearningResultModel>> Handle(GetReportLearningResultQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<DashBoardLearningResultModel>();
            var studentIds = new List<Guid>();
            bool isRoleAdminSchool = _authContext.Roles != null && _authContext.Roles.Contains(EnumRole.AdminSchool.ToString());
            if (isRoleAdminSchool)
            {
                var userResults = await _userService.GetStudentsDashboardAsync(new GetStudentsDashboardQueryModel());
                if (!userResults.IsSuccessStatusCode)
                {
                    methodResult.AddError(userResults.Error);
                    return methodResult;
                }
                studentIds = userResults.Content?.Result?.Select(x => x.Id).ToList() ?? new List<Guid>();
            }
            var courseOveralls = await (from baseQ in _courseResultRepository.Queryable
                                        join c in _courseRepository.Queryable on baseQ.CourseId equals c.Id
                                        join cum in _courseUnitMockTestRepository.Queryable on c.Id equals cum.CourseId
                                        join ur in _unitResultRepository.Queryable on new { baseQ.StudentId, baseQ.CourseId, UnitId = cum.UnitId } equals new { ur.StudentId, ur.CourseId, UnitId = (Guid?)ur.UnitId }
                                        where baseQ.WorkingStatus == EnumWorkingStatus.Active && ur.Status == EnumResultStatus.Done &&
                                        (!isRoleAdminSchool || (!studentIds.Any() || studentIds.Contains(baseQ.StudentId))) &&
                                        (!request.EndDate.HasValue || (ur.UpdatedDate ?? ur.CreatedDate).Date <= request.EndDate.Value.Date)
                                        group new { baseQ, ur, c } by new { baseQ.CourseId, baseQ.StudentId } into g
                                        select new
                                        {
                                            StudentId = g.Key.StudentId,
                                            CourseLevel = g.Select(x => x.c).Select(x => x.CourseLevel).FirstOrDefault(),
                                            Percent = g.Select(x => x.ur).Any() ? g.Select(x => x.ur).Average(x => x.Percent) : default(double),
                                        }).ToListAsync(cancellationToken);

            var unitOveralls = await (from baseQ in _courseResultRepository.Queryable
                                      join c in _courseRepository.Queryable on baseQ.CourseId equals c.Id
                                      join cum in _courseUnitMockTestRepository.Queryable on c.Id equals cum.CourseId
                                      join ur in _unitResultRepository.Queryable on new { baseQ.StudentId, baseQ.CourseId, UnitId = cum.UnitId } equals new { ur.StudentId, ur.CourseId, UnitId = (Guid?)ur.UnitId }
                                      where baseQ.WorkingStatus == EnumWorkingStatus.Active &&
                                       (!isRoleAdminSchool || ((studentIds == null || !studentIds.Any()) || studentIds.Contains(baseQ.StudentId))) &&
                                      (!request.EndDate.HasValue || (ur.UpdatedDate ?? ur.CreatedDate).Date <= request.EndDate.Value.Date)
                                      group new { cum, ur, c } by new { cum.Number, c.CourseLevel } into g
                                      select new
                                      {
                                          DisplayOrder = g.Key.Number,
                                          CourseLevel = g.Key.CourseLevel,
                                          CountPercent = g.Select(x => x.ur).Where(x => x.Status == EnumResultStatus.Done).Count(),
                                          TotalPercent = g.Select(x => x.ur).Where(x => x.Status == EnumResultStatus.Done).Sum(x => x.Percent),
                                      }).ToListAsync(cancellationToken);

            DashBoardLearningResultModel reportLearningResult = new DashBoardLearningResultModel
            {
                Percent = (int)NumberHelper.ConvertRound(courseOveralls.Any() ? courseOveralls.Average(x => x.Percent) : default),
                LearningResultChart = new LearningResultChartModel
                {
                    Type = EnumChartType.PieChart,
                    LearningResultDatas = ConvertHelper.EnumToList<EnumOverallScore>().Select(enumScore =>
                    {
                        var percent = (int)EnumOverallScore.Accuracy75OrMore;
                        var overallPercents = courseOveralls.Where(x => enumScore == EnumOverallScore.Accuracy75OrMore ? x.Percent >= percent : x.Percent < percent);
                        return new LearningResultDataChartModel
                        {
                            Label = enumScore.ToString(),
                            TotalStudentAca = overallPercents.Count(x => x.CourseLevel.GetEnumCourseType() == EnumCourseType.Academic),
                            TotalStudentIELST = overallPercents.Count(x => x.CourseLevel.GetEnumCourseType() == EnumCourseType.Ielts),
                            Value = (int)NumberHelper.ConvertPercentDouble((double)overallPercents.Count() / courseOveralls.Count),
                        };
                    }).ToList()
                },
                OverallUnitChart = new LearningResultUnitChartModel
                {
                    Type = EnumChartType.LineChart,
                    UnitCharts = ConvertHelper.EnumToList<EnumCourseType>().Select(courseType =>
                    {
                        return new UnitChartModel
                        {
                            CourseType = courseType,
                            DataColumns = unitOveralls.Where(x => x.CourseLevel.GetEnumCourseType() == courseType)
                            .GroupBy(x => x.DisplayOrder)
                            .OrderBy(x => x.Key).Select(x => new DataChartModel
                            {
                                Label = $"{x.Key}",
                                Value = x.Where(y => y.CountPercent != 0).Any() ? (int)NumberHelper.ConvertRound(x.Where(x => x.CountPercent != 0).Average(y => y.TotalPercent / y.CountPercent)) : default,
                            }).ToList(),
                        };
                    }).ToList()
                }
            };

            methodResult.Result = reportLearningResult;
            return methodResult;
        }
    }
}
