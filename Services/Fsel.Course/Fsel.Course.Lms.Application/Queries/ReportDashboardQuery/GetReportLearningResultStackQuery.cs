// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.ReportDashboardQuery
{
    using System.Text.Json.Serialization;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Helpers;
    using Fsel.Core.Extensions;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels.BaseChartModels;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Course.Lms.Application.Services.UserServices.QueryModels;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Helpers;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class GetReportLearningResultStackQuery : IRequest<MethodResult<StackBarChartsModel>>
    {
        public DateTime? EndDate { get; set; }
        public string? SchoolClassStr { get; set; }
        public string? CourseTypeStr { get; set; }
        public string? ScoreOverallStr { get; set; }

        [JsonIgnore]
        public IList<EnumCourseType>? CourseTypes
        {
            get
            {
                return CourseTypeStr.ToList<EnumCourseType>();
            }
        }

        [JsonIgnore]
        public IList<EnumOverallScore>? ScoreOveralls
        {
            get
            {
                return ScoreOverallStr.ToList<EnumOverallScore>();
            }
        }
    }

    public class GetReportLearningResultStackQueryHandler : IRequestHandler<GetReportLearningResultStackQuery, MethodResult<StackBarChartsModel>>
    {
        private readonly IUserService _userService;
        private readonly IUnitResultRepository _unitResultRepository;
        private readonly ICourseResultRepository _courseResultRepository;
        private readonly ICourseUnitMockTestRepository _courseUnitMockTestRepository;

        public GetReportLearningResultStackQueryHandler(
            IUserService userService,
            IUnitResultRepository unitResultRepository,
            ICourseResultRepository courseResultRepository,
            ICourseUnitMockTestRepository courseUnitMockTestRepository)
        {
            _userService = userService;
            _unitResultRepository = unitResultRepository;
            _courseResultRepository = courseResultRepository;
            _courseUnitMockTestRepository = courseUnitMockTestRepository;
        }

        public async Task<MethodResult<StackBarChartsModel>> Handle(GetReportLearningResultStackQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<StackBarChartsModel>();
            var courseLevels = request.CourseTypes?.SelectMany(x => EnumCourseLevelHelper.GetEnumCourseLevels(x)).ToList() ?? new List<EnumCourseLevel>();
            var userResults = await _userService.GetStudentsDashboardAsync(new GetStudentsDashboardQueryModel
            {
                EnumCourseLevelStr = string.Join(",", courseLevels),
                SchoolClassStr = request.SchoolClassStr,
            });
            if (!userResults.IsSuccessStatusCode)
            {
                methodResult.AddError(userResults.Error);
                return methodResult;
            }
            var studentGroups = userResults.Content?.Result?
                .Where(x => !string.IsNullOrEmpty(x.SchoolClass))
                .GroupBy(x => x.SchoolClass)
                .OrderBy(x =>
                {
                    // Tách phần số ra
                    var numberPart = new string(x.Key.Where(char.IsDigit).ToArray());
                    if (long.TryParse(numberPart, out var parsedNumber))
                    {
                        return parsedNumber; // Sắp xếp theo giá trị số
                    }
                    return long.MaxValue; // Nếu không phải số, đặt giá trị lớn nhất
                })
                .ThenBy(y =>
                {
                    // Tách phần chữ cái sau số
                    var letterPart = new string(y.Key.SkipWhile(char.IsDigit).ToArray());
                    return letterPart;
                }).Take(8)
                .ToList();
            var studentIds = studentGroups?.SelectMany(x => x.Select(y => y.Id)).ToList();

            var courseOveralls = await (from baseQ in _courseResultRepository.Queryable
                                        join cum in _courseUnitMockTestRepository.Queryable on baseQ.CourseId equals cum.CourseId
                                        join ur in _unitResultRepository.Queryable on new { baseQ.StudentId, baseQ.CourseId, UnitId = cum.UnitId } equals new { ur.StudentId, ur.CourseId, UnitId = (Guid?)ur.UnitId }
                                        where baseQ.WorkingStatus == EnumWorkingStatus.Active && ur.Status == EnumResultStatus.Done && studentIds.Contains(baseQ.StudentId) &&
                                        (!request.EndDate.HasValue || (ur.UpdatedDate ?? ur.CreatedDate).Date <= request.EndDate.Value.Date)
                                        group new { baseQ, ur } by new { baseQ.CourseId, baseQ.StudentId } into g
                                        select new
                                        {
                                            StudentId = g.Key.StudentId,
                                            Percent = g.Select(x => x.ur).Any() ? g.Select(x => x.ur).Average(x => x.Percent) : default(double),
                                        }).ToListAsync(cancellationToken);

            StackBarChartsModel reportLearningResult = new StackBarChartsModel
            {
                Type = EnumChartType.StackbarChart,
                DataCharts = studentGroups?.Select(studentGroup => new StackBarChartModel
                {
                    Label = studentGroup.Key,
                    DataColumns = ConvertHelper.EnumToList<EnumOverallScore>()
                    .Where(enumOverall => request.ScoreOveralls == null || request.ScoreOveralls.Contains(enumOverall))
                    .Select(enumOverall =>
                    {
                        var percent = (int)EnumOverallScore.Accuracy75OrMore;
                        var overallPercents = courseOveralls.Where(x => enumOverall == EnumOverallScore.Accuracy75OrMore ? x.Percent >= percent : x.Percent < percent);
                        return new DataChartModel
                        {
                            Label = enumOverall.ToString(),
                            Value = overallPercents.Where(y => studentGroup.Select(z => z.Id).Contains(y.StudentId)).Count(),
                        };
                    }).ToList()
                }).ToList() ?? new List<StackBarChartModel>()
            };
            methodResult.Result = reportLearningResult;
            return methodResult;
        }
    }
}
