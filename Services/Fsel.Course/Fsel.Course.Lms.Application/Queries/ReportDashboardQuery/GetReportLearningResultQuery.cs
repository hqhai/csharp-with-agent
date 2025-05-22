// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.ReportDashboardQuery
{
    using System.Linq;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Helpers;
    using Fsel.Core.Base;
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
        private readonly CourseDbContext _courseDbContext;

        public GetReportLearningResultQueryHandler(IUserService userService,
                                                   IUnitResultRepository unitResultRepository,
                                                   ICourseResultRepository courseResultRepository,
                                                   ICourseRepository courseRepository,
                                                   ICourseUnitMockTestRepository courseUnitMockTestRepository,
                                                   AuthContext authContext,
                                                   CourseDbContext courseDbContext)
        {
            _userService = userService;
            _unitResultRepository = unitResultRepository;
            _courseResultRepository = courseResultRepository;
            _courseRepository = courseRepository;
            _courseUnitMockTestRepository = courseUnitMockTestRepository;
            _authContext = authContext;
            _courseDbContext = courseDbContext;
        }

        public async Task<MethodResult<DashBoardLearningResultModel>> Handle(GetReportLearningResultQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<DashBoardLearningResultModel>();
            //var userResults = await _userService.GetStudentsDashboardAsync(new GetStudentsDashboardQueryModel());
            //if (!userResults.IsSuccessStatusCode)
            //{
            //    methodResult.AddError(userResults.Error);
            //    return methodResult;
            //}
            //var studentIds = userResults.Content?.Result?.Select(x => x.Id).ToList() ?? new List<Guid>();
            var schoolIdResult = await _userService.GetSchoolIdAsync();
            var schoolId = schoolIdResult.Content?.Result ?? Guid.NewGuid();
            var endDate = request.EndDate ?? (object)DBNull.Value;

            var courseOveralls = await _courseDbContext.Set<ReportLearningResultModel>()
                                   .FromSqlRaw("EXEC DashBoardStudentResult @SchoolId, @EndDate",
                                        new SqlParameter("@SchoolId", schoolId),
                                        new SqlParameter("@EndDate", endDate))
                                   .AsNoTracking().ToListAsync(cancellationToken);

            //var courseOverallas = await (from baseQ in _courseResultRepository.Queryable
            //                             join c in _courseRepository.Queryable on baseQ.CourseId equals c.Id
            //                             join cum in _courseUnitMockTestRepository.Queryable on c.Id equals cum.CourseId
            //                             join ur in _unitResultRepository.Queryable on new { baseQ.StudentId, baseQ.CourseId, UnitId = cum.UnitId } equals new { ur.StudentId, ur.CourseId, UnitId = (Guid?)ur.UnitId }
            //                             where baseQ.WorkingStatus == EnumWorkingStatus.Active && ur.Status == EnumResultStatus.Done && studentIds.Contains(baseQ.StudentId) &&
            //                             (!request.EndDate.HasValue || (ur.UpdatedDate ?? ur.CreatedDate).Date <= request.EndDate.Value.Date)
            //                             group new { baseQ, ur, c } by new { baseQ.CourseId, baseQ.StudentId } into g
            //                             select new
            //                             {
            //                                 StudentId = g.Key.StudentId,
            //                                 CourseLevel = g.Select(x => x.c).Select(x => x.CourseLevel).FirstOrDefault(),
            //                                 Percent = g.Select(x => x.ur).Any() ? Math.Round(g.Select(x => x.ur).Average(x => x.Percent)) : default(double),
            //                             }).ToListAsync(cancellationToken);

            var unitOveralls = await _courseDbContext.Set<ReportLearningResultModel>()
                            .FromSqlRaw("EXEC DashBoardUnitStudentResult @SchoolId , @EndDate",
                                 new SqlParameter("@SchoolId", schoolId),
                                 new SqlParameter("@EndDate", endDate))
                            .AsNoTracking().ToListAsync(cancellationToken);
            //var unitOverall1s = await (from baseQ in _courseResultRepository.Queryable.WhereBulkContains(studentIds, x => x.StudentId)
            //                           join c in _courseRepository.Queryable on baseQ.CourseId equals c.Id
            //                           join cum in _courseUnitMockTestRepository.Queryable on c.Id equals cum.CourseId
            //                           join ur in _unitResultRepository.Queryable on new { baseQ.StudentId, baseQ.CourseId, UnitId = cum.UnitId } equals new { ur.StudentId, ur.CourseId, UnitId = (Guid?)ur.UnitId }
            //                           where baseQ.WorkingStatus == EnumWorkingStatus.Active &&
            //                           (!request.EndDate.HasValue || (ur.UpdatedDate ?? ur.CreatedDate).Date <= request.EndDate.Value.Date)
            //                           group new { cum, ur, c } by new { cum.Number, c.CourseLevel } into g
            //                           select new
            //                           {
            //                               DisplayOrder = g.Key.Number,
            //                               CourseLevel = g.Key.CourseLevel,
            //                               Percents = g.Select(x => x.ur).Where(x => x.Status == EnumResultStatus.Done).Select(x => x.Percent).ToList(),
            //                           }).ToListAsync(cancellationToken);

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
                            TotalStudentAca = overallPercents.Count(x => x.CourseType == EnumCourseType.Academic),
                            TotalStudentIELST = overallPercents.Count(x => x.CourseType == EnumCourseType.Ielts),
                            Value = (int)NumberHelper.GetPercent(overallPercents.Count(), courseOveralls.Count),
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
                            DataColumns = Enumerable.Range(0, courseType == EnumCourseType.Ielts ? CourseProgressValue.CountUnitIELTS : courseType == EnumCourseType.Academic ? CourseProgressValue.CountUnitAca : ValueDefault).Select(item =>
                            {
                                var indexUnit = item + 1;
                                var overallPercentUnits = unitOveralls.Where(x => x.CourseType == courseType && x.DisplayOrder == indexUnit).Select(x => x.Percent).ToList();
                                var percent = overallPercentUnits.Any() ? NumberHelper.ConvertRound(overallPercentUnits.Average()) : ValueDefault;
                                return new DataChartModel
                                {
                                    Label = $"{indexUnit}",
                                    Value = (int)percent
                                };
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
