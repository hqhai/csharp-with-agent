// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.ReportDashboardQuery
{
    using System.Text.Json.Serialization;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Helpers;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels.BaseChartModels;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Course.Lms.Application.Services.UserServices.QueryModels;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Helpers;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class GetReportLearningProgressStackQuery : IRequest<MethodResult<StackBarChartsModel>>
    {
        public DateTime? EndDate { get; set; }
        public string? SchoolClassStr { get; set; }
        public string? EnumCourseTypeStr { get; set; }
        public string? EnumCourseLevelStr { get; set; }
        public string? LearningProgressStr { get; set; }

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

        [JsonIgnore]
        public IList<EnumLearningProgress>? LearningProgresses
        {
            get
            {
                return LearningProgressStr.ToList<EnumLearningProgress>();
            }
        }
    }

    public class GetReportLearningProgressStackQueryHandler : IRequestHandler<GetReportLearningProgressStackQuery, MethodResult<StackBarChartsModel>>
    {
        private readonly IUserService _userService;
        private readonly ICourseResultRepository _courseResultRepository;
        private readonly ICourseRepository _courseRepository;
        private readonly ILessonResultRepository _lessonResultRepository;

        public GetReportLearningProgressStackQueryHandler(IUserService userService,
                                                   ICourseResultRepository courseResultRepository,
                                                   ICourseRepository courseRepository,
                                                   ILessonResultRepository lessonResultRepository)
        {
            _userService = userService;
            _courseResultRepository = courseResultRepository;
            _courseRepository = courseRepository;
            _lessonResultRepository = lessonResultRepository;
        }

        public async Task<MethodResult<StackBarChartsModel>> Handle(GetReportLearningProgressStackQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<StackBarChartsModel>();
            var courseLevels = request.CourseTypes.GetCourseLevels(request.CourseLevels);
            bool isMaxHoursCompleted = true;
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
            var studentIds = studentGroups?.SelectMany(x => x.Select(y => y.Id)).ToList() ?? new List<Guid>();

            var queryChart = await (from baseQ in _courseResultRepository.Queryable.WhereBulkContains(studentIds, x => x.StudentId)
                                    join c in _courseRepository.Queryable on baseQ.CourseId equals c.Id
                                    join lr in _lessonResultRepository.Queryable on new { baseQ.StudentId, baseQ.CourseId } equals new { lr.StudentId, lr.CourseId }
                                    where baseQ.WorkingStatus == EnumWorkingStatus.Active &&
                                    (!request.EndDate.HasValue || (lr.UpdatedDate ?? lr.CreatedDate).Date <= request.EndDate.Value.Date) &&
                                    ((courseLevels == null || !courseLevels.Any()) || courseLevels.Contains(c.CourseLevel)) &&
                                    lr.Status == EnumResultStatus.Done
                                    group new { lr } by new { baseQ.StudentId } into g
                                    select new
                                    {
                                        StudentId = g.Key.StudentId,
                                        IsOnSchedule = isMaxHoursCompleted
                                        //g.OrderByDescending(x => x.lr.CompletionDate).ThenByDescending(x => x.lr.UpdatedDate).Select(x => !x.lr.MaxHoursCompleted.HasValue || ((x.lr.CompletionDate ?? x.lr.UpdatedDate) - x.lr.NewDate).GetValueOrDefault().TotalSeconds <= x.lr.MaxHoursCompleted).FirstOrDefault()
                                    }).ToListAsync(cancellationToken);

            StackBarChartsModel reportLearningResult = new StackBarChartsModel
            {
                Type = EnumChartType.StackbarChart,
                DataCharts = studentGroups?.Select(studentGroup => new StackBarChartModel
                {
                    Label = studentGroup.Key,
                    DataColumns = ConvertHelper.EnumToList<EnumLearningProgress>()
                    .Where(learningProgress => request.LearningProgresses == null || request.LearningProgresses.Contains(learningProgress))
                    .Select(learningProgress =>
                    {
                        var overallStudents = queryChart.Where(x => Convert.ToInt32(x.IsOnSchedule) == (int)learningProgress);
                        return new DataChartModel
                        {
                            Label = learningProgress.ToString(),
                            Value = overallStudents.Where(y => studentGroup.Select(z => z.Id).Contains(y.StudentId)).Count(),
                        };
                    }).ToList()
                }).ToList() ?? new List<StackBarChartModel>()
            };
            methodResult.Result = reportLearningResult;
            return methodResult;
        }
    }
}
