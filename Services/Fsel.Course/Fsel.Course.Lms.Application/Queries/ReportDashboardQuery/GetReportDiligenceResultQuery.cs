// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.ReportDashboardQuery
{
    using System.Globalization;
    using System.Text.Json.Serialization;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Common.Helpers;
    using Fsel.Common.Models;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Course.Domain.Models.EntityModels.BaseChartModels;
    using Fsel.Course.Lms.Application.Services.SystemService;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Helpers;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class GetReportDiligenceResultQuery : IRequest<MethodResult<DashBoardDiligenceModel>>
    {
        public DateTime ReportStartDate { get; set; }

        public DateTime? ReportEndDate { get; set; }

        public int ReportDay { get; set; }

        public string? EnumCourseTypesStr { get; set; }

        public string? SchoolClassesStr { get; set; }

        [JsonIgnore]
        public IList<EnumCourseType>? EnumCourseTypes
        {
            get
            {
                return EnumCourseTypesStr.ToList<EnumCourseType>();
            }
        }

        [JsonIgnore]
        public IList<string>? SchoolClasses
        {
            get
            {
                return SchoolClassesStr.ToList<string>();
            }
        }
    }

    public class GetReportDiligenceResultQueryHandler : IRequestHandler<GetReportDiligenceResultQuery, MethodResult<DashBoardDiligenceModel>>
    {
        private readonly IUserService _userService;
        private readonly ISystemService _systemService;

        public GetReportDiligenceResultQueryHandler(IUserService userService, ISystemService systemService)
        {
            _userService = userService;
            _systemService = systemService;
        }

        public async Task<MethodResult<DashBoardDiligenceModel>> Handle(GetReportDiligenceResultQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<DashBoardDiligenceModel>();

            var resultModel = new DashBoardDiligenceModel();

            var studentResult = await _userService.GetStudentsBySchoolId();

            if (!studentResult.IsSuccessStatusCode)
            {
                methodResult.AddError(studentResult.Error);
                return methodResult;
            }
            var student = studentResult.Content?.Result;


            if (student == null || student.Count == 0)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(student));
                return methodResult;
            }

            student = student.Where(s => s.Human != null).ToList();

            var accessTimeResult = await _systemService.GetListFeatureAccessTime(new BaseQueryModel()
            {
                Filters = new List<GenericFilterModel>() {
                    new GenericFilterModel()
                    {
                        Property = "CreatedUserId",
                        Operator = EnumFilterOperator.In,
                        Value = student!.Select(s => s.Human!.UserId).ToList()
                    },
                    new GenericFilterModel()
                    {
                        Property = "EnumFeature",
                        Operator = EnumFilterOperator.In,
                        Value = new List<EnumFeature>() { EnumFeature.ClassForum, EnumFeature.VideoLesson, EnumFeature.HomeWork }
                    }
                },
            });

            if (!accessTimeResult.IsSuccessStatusCode)
            {
                methodResult.AddError(accessTimeResult.Error);
                return methodResult;
            }

            var featureAccsessTime = accessTimeResult.Content?.Result;

            if (featureAccsessTime == null || featureAccsessTime.Count == 0)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(featureAccsessTime));
                return methodResult;
            }

            resultModel.SchoolName = student.FirstOrDefault()?.School;

            if (request.EnumCourseTypes != null && request.EnumCourseTypes.Count > 0)
            {
                student = student.Where(s => request.EnumCourseTypes.Contains(s.CourseLevel.GetEnumCourseType())).ToList();
                featureAccsessTime = featureAccsessTime.Where(f => student.Select(s => s.Human!.UserId).Contains(f.CreatedUserId)).ToList();
            }

            if (request.SchoolClasses != null && request.SchoolClasses.Count > 0)
            {
                student = student.Where(s => s.SchoolClass != null && request.SchoolClasses.Contains(s.SchoolClass)).ToList();
                featureAccsessTime = featureAccsessTime.Where(f => student.Select(s => s.Human!.UserId).Contains(f.CreatedUserId)).ToList();
            }

            #region Biểu đồ học sinh không truy cập
            var reportDate = DateTime.UtcNow.AddDays(-request.ReportDay + 1).Date;

            var featureAccsessTimeInSomeDays = featureAccsessTime.Where(f => f.LastVisited.HasValue && f.LastVisited.Value.Date >= reportDate);

            var studentNotAccessInSomeDays = student
                .Where(s => !featureAccsessTimeInSomeDays.Any(f => f.CreatedUserId == s.Human!.UserId))
                .Where(s => s.SchoolClass != null)
                .GroupBy(s => s.SchoolClass)
                .OrderBy(x => x.Key)
                .Select(x => new DataChartModel()
                {
                    Label = x.Key,
                    Value = x.Count()
                }).ToList();

            resultModel.NumberStudentNotAccessModel.Type = EnumChartType.BarChart;
            resultModel.NumberStudentNotAccessModel.DataCharts = studentNotAccessInSomeDays;

            #endregion

            #region Biểu đồ số lượng học sinh truy cập hệ thống
            featureAccsessTimeInSomeDays = featureAccsessTime
                    .Where(f =>
                        f.LastVisited.HasValue &&
                        (request.ReportEndDate.HasValue ?
                        request.ReportStartDate.Date <= f.LastVisited.Value.ConvertTimeFromUtc(EnumCountryKey.Vietnam).Date &&
                        request.ReportEndDate.Value.Date >= f.LastVisited.Value.ConvertTimeFromUtc(EnumCountryKey.Vietnam).Date :
                        request.ReportStartDate.Date == f.LastVisited.Value.ConvertTimeFromUtc(EnumCountryKey.Vietnam).Date));

            if (request.ReportEndDate.HasValue)
            {
                var studentAccessModelThisWeek = featureAccsessTimeInSomeDays
                    .Select(f => new
                    {
                        Date = f.LastVisited!.Value.ConvertTimeFromUtc(EnumCountryKey.Vietnam).Date,
                        UserId = f.CreatedUserId
                    })
                    .GroupBy(x => x.Date).OrderBy(x => x.Key)
                    .Select(x => new DataChartModel()
                    {
                        Label = x.Key.ToString(CultureInfo.CurrentCulture),
                        Value = x.DistinctBy(x => x.UserId).Count()
                    }).ToList();

                double totalStudentAccessModelThisWeek = studentAccessModelThisWeek.Sum(x => x.Value);
                double totalStudentAccessModelLastWeek = featureAccsessTime
                    .Where(f =>
                        f.LastVisited.HasValue &&
                        (request.ReportStartDate.Date.AddDays(-7) <= f.LastVisited.Value.ConvertTimeFromUtc(EnumCountryKey.Vietnam).Date &&
                        request.ReportEndDate.Value.Date.AddDays(-7) >= f.LastVisited.Value.ConvertTimeFromUtc(EnumCountryKey.Vietnam).Date))
                    .Select(f => new
                    {
                        Date = f.LastVisited!.Value.ConvertTimeFromUtc(EnumCountryKey.Vietnam).Date,
                        UserId = f.CreatedUserId
                    })
                    .GroupBy(x => x.Date)
                    .Select(x => x.DistinctBy(x => x.UserId).Count())
                    .Sum();

                resultModel.NumberStudentAccessModel.DataCharts = studentAccessModelThisWeek;
                resultModel.NumberStudentAccessModel.Percent = totalStudentAccessModelLastWeek == 0 ? 100 : (int)(((totalStudentAccessModelThisWeek - totalStudentAccessModelLastWeek) / totalStudentAccessModelLastWeek) * 100);
            }
            else
            {
                var studentAccessModelThisDay = featureAccsessTimeInSomeDays
                    .Select(f => new
                    {
                        Hour = AssignHourLabelForAccessTime(f.LastVisited),
                        UserId = f.CreatedUserId
                    })
                    .GroupBy(x => x.Hour).OrderBy(x => (int)x.Key)
                    .Select(x => new DataChartModel()
                    {
                        Label = x.Key.GetDescription(),
                        Value = x.DistinctBy(x => x.UserId).Count()
                    });

                double totalAccessThisDay = studentAccessModelThisDay.Sum(x => x.Value);
                double totalAccessYesterday = featureAccsessTime
                    .Where(f =>
                        f.LastVisited.HasValue &&
                        request.ReportStartDate.Date.AddDays(-1) == f.LastVisited.Value.ConvertTimeFromUtc(EnumCountryKey.Vietnam).Date)
                    .Select(f => new
                    {
                        Hour = AssignHourLabelForAccessTime(f.LastVisited),
                        UserId = f.CreatedUserId
                    })
                    .GroupBy(x => x.Hour)
                    .Select(x => x.DistinctBy(x => x.UserId).Count())
                    .Sum();

                resultModel.NumberStudentAccessModel.DataCharts = studentAccessModelThisDay.ToList();
                resultModel.NumberStudentAccessModel.Percent = totalAccessYesterday == 0 ? 100 : (int)(((totalAccessThisDay - totalAccessYesterday) / totalAccessYesterday) * 100);
            }

            resultModel.NumberStudentAccessModel.Type = EnumChartType.LineChart;

            #endregion


            #region Biểu đồ Báo các học tập theo tuần
            var dayOfWeek = DateTime.UtcNow.DayOfWeek;
            double totalDays = DateTime.UtcNow.DayOfWeek == DayOfWeek.Sunday ? 7 : (int)DateTime.UtcNow.DayOfWeek;
            var startCalculateDay = DateTime.UtcNow.Date.AddDays(dayOfWeek == DayOfWeek.Sunday ? -6 : -(int)dayOfWeek + 1);

            double totalTimeThisWeek = 0;
            var featureAccsessTimeThisWeek = featureAccsessTime
                .Where(f => f.LastVisited.HasValue &&
                startCalculateDay <= f.LastVisited.Value.Date && DateTime.UtcNow.Date >= f.LastVisited.Value.Date
                )
                .Select(f =>
                {
                    totalTimeThisWeek += f.AccessTime;
                    return new
                    {
                        Index = (f.LastVisited!.Value.DayOfWeek == DayOfWeek.Sunday) ? 7 : (int)f.LastVisited!.Value.DayOfWeek,
                        DayOfWeek = f.LastVisited!.Value.DayOfWeek,
                        EnumFeature = f.EnumFeature,
                        AccessTime = f.AccessTime
                    };
                })
                .GroupBy(f => new { f.DayOfWeek, f.Index })
                .OrderBy(f => f.Key.Index)
                .Select(f => new StackBarChartModel()
                {
                    Label = f.Key.DayOfWeek.ToString(),
                    DataColumns = f.ToList().GroupBy(x => x.EnumFeature).Select(x => new DataChartModel()
                    {
                        Label = x.Key.ToString(),
                        Value = (int)x.Sum(x => x.AccessTime)
                    }).ToList()
                });

            double totalAccsessTimeLastWeek = featureAccsessTime
                .Where(f => f.LastVisited.HasValue &&
                startCalculateDay.AddDays(-7) <= f.LastVisited.Value.Date && DateTime.UtcNow.Date.AddDays(-7) >= f.LastVisited.Value.Date
                ).Sum(f => f.AccessTime);
            totalAccsessTimeLastWeek = totalAccsessTimeLastWeek / totalDays;

            resultModel.LearningResultReportModel.Type = EnumChartType.StackbarChart;
            resultModel.LearningResultReportModel.DataCharts = featureAccsessTimeThisWeek.ToList();
            resultModel.LearningResultReportModel.TotalTime = (int)totalTimeThisWeek;
            resultModel.LearningResultReportModel.AveragePerDay = totalTimeThisWeek / totalDays;
            resultModel.LearningResultReportModel.Percent = totalAccsessTimeLastWeek == 0 ? 100 : (int)(((resultModel.LearningResultReportModel.AveragePerDay - totalAccsessTimeLastWeek) / totalAccsessTimeLastWeek) * 100);

            #endregion

            methodResult.Result = resultModel;
            methodResult.StatusCode = StatusCodes.Status200OK;

            return methodResult;
        }

        private static EnumHour AssignHourLabelForAccessTime(DateTime? date)
        {
            if (!date.HasValue)
            {
                return default;
            }

            date = date.Value.ConvertTimeFromUtc(EnumCountryKey.Vietnam);

            foreach (EnumHour hour in Enum.GetValues(typeof(EnumHour)))
            {
                var index = (int)hour * 2;
                if (index <= date.Value.Hour && index + 2 > date.Value.Hour)
                {
                    return hour;
                }
                continue;
            }

            return default;
        }
    }
}
