// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.ReportDashboardQuery
{
    using System;
    using System.Globalization;
    using System.Text.Json.Serialization;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Helpers;
    using Fsel.Course.Domain.Models.EntityModels.BaseChartModels;
    using Fsel.Course.Lms.Application.Services.SystemService;
    using Fsel.Course.Lms.Application.Services.SystemService.Models;
    using Fsel.Course.Lms.Application.Services.SystemService.QueryModels;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Course.Lms.Application.Services.UserServices.Models;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Helpers;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class GetReportDiligenceResultQuery : IRequest<MethodResult<DashBoardDiligenceModel>>
    {
        public DateTime ReportStartDate { get; set; }

        public DateTime? ReportEndDate { get; set; }

        public int ReportDay { get; set; }

        public string? EnumCourseLevelsStr { get; set; }

        public string? SchoolClassesStr { get; set; }

        [JsonIgnore]
        public IList<EnumCourseLevel>? EnumCourseLevels
        {
            get
            {
                return EnumCourseLevelsStr.ToList<EnumCourseLevel>();
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

        /// <summary>
        ///  Khoảng thời gian để chia ra các khoảng trong ngày
        /// </summary>
        private const int TimePeriod = 2;

        /// <summary>
        /// Số ngày trong tuần
        /// </summary>
        private const int NumberDayOfWeek = 7;

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

            // Lấy dữ liệu học sinh
            var studentResult = await _userService.GetStudentsBySchoolId();

            if (!studentResult.IsSuccessStatusCode)
            {
                methodResult.AddError(studentResult.Error);
                return methodResult;
            }
            var students = studentResult.Content?.Result;

            if (students == null || !students.Any())
            {
                return methodResult;
            }

            // Lọc bản ghi có dữ liệu Human
            students = students.Where(s => s.User != null).ToList();

            // Lấy dữ liệu AccessTime
            var accessTimeResult = await _systemService.GetAccessTimeByUserAndFeature(new GetAccessTimeByUserAndFeatureQueryModel
            {
                UserIds = students.Select(s => s.UserId).ToList(),
                Features = new List<EnumFeature>() { EnumFeature.ClassForum, EnumFeature.VideoLesson, EnumFeature.HomeWork }
            });

            // Danh sách enum feature
            var enumFeatures = new List<string>()
            {
                EnumFeature.ClassForum.ToString(),
                EnumFeature.VideoLesson.ToString(),
                EnumFeature.HomeWork.ToString()
            };

            if (!accessTimeResult.IsSuccessStatusCode)
            {
                methodResult.AddError(accessTimeResult.Error);
                return methodResult;
            }

            var featureAccsessTime = accessTimeResult.Content?.Result;

            if (featureAccsessTime == null || featureAccsessTime.Count == 0)
            {
                return methodResult;
            }

            // Gán tên trường học
            resultModel.SchoolName = students.FirstOrDefault()?.School;

            // Lọc thep chương trình học
            if (request.EnumCourseLevels != null && request.EnumCourseLevels.Count > 0)
            {
                students = students.Where(s => s.CourseLevel != null && request.EnumCourseLevels.Contains(s.CourseLevel ?? default)).ToList();
                featureAccsessTime = featureAccsessTime.Where(f => students.Select(s => s.UserId).Contains(f.CreatedUserId)).ToList();
            }

            // Lọc theo lớp học của học sinh
            if (request.SchoolClasses != null && request.SchoolClasses.Count > 0)
            {
                students = students.Where(s => s.SchoolClass != null && request.SchoolClasses.Contains(s.SchoolClass)).ToList();
                featureAccsessTime = featureAccsessTime.Where(f => students.Select(s => s.UserId).Contains(f.CreatedUserId)).ToList();
            }

            var classes = students.Where(s => s.SchoolClass != null).Select(s => s.SchoolClass).ToList();

            resultModel.NumberStudentNotAccessModel = GetStudentNotAccessModel(students, featureAccsessTime, request, classes);

            resultModel.NumberStudentAccessModel = GetStudentAccessModel(featureAccsessTime, request);

            resultModel.LearningResultReportModel = GetLearningResultReportModel(featureAccsessTime, enumFeatures);

            methodResult.Result = resultModel;
            methodResult.StatusCode = StatusCodes.Status200OK;

            return methodResult;
        }

        /// <summary>
        /// Lấy thông tin báo cáo kết quả theo tuần
        /// </summary>
        /// <param name="featureAccsessTime">Dữ liệu truy cập hệ thống của học sinh</param>
        /// <returns></returns>
        private static LearningResultReportModel GetLearningResultReportModel(IList<FeatureAccessTimeModel> featureAccsessTime, IList<string>? enumFeatures)
        {
            // Khởi tạo Model
            // Nhãn thứ 1 model sẽ là các thứ trong tuần
            // Nhãn thứ 2 là các dạng EnumFeature
            var resultModel = new LearningResultReportModel()
            {
                Type = EnumChartType.StackbarChart
            };

            var dayOfWeek = DateTime.UtcNow.DayOfWeek;
            double totalDays = DateTime.UtcNow.DayOfWeek == DayOfWeek.Sunday ? NumberDayOfWeek : (int)DateTime.UtcNow.DayOfWeek;

            // Xóa định ngày bắt đầu lọc
            var startCalculateDay = DateTime.UtcNow.Date.AddDays(dayOfWeek == DayOfWeek.Sunday ? (-NumberDayOfWeek + 1) : -(int)dayOfWeek + 1);

            double totalTimeThisWeek = 0;

            var dayOfWeeks = Enum.GetValues(typeof(DayOfWeek)).Cast<DayOfWeek>().Select(day => day.ToString()).ToList();

            // Lọc dữ liệu AccessTime
            var featureAccsessTimeThisWeek = featureAccsessTime
                .Where(f => f.LastVisited.HasValue &&
                startCalculateDay <= f.LastVisited.Value.Date && DateTime.UtcNow.Date >= f.LastVisited.Value.Date
                )
                .Select(f =>
                {
                    totalTimeThisWeek += f.AccessTime;
                    return new
                    {
                        Index = (f.LastVisited!.Value.DayOfWeek == DayOfWeek.Sunday) ? NumberDayOfWeek : (int)f.LastVisited!.Value.DayOfWeek, // Sử dụng để sắp xếp
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
                    }).OrderBy(f => f.Label).ToList()
                }).ToList();

            foreach (var model in featureAccsessTimeThisWeek)
            {
                if (model.DataColumns != null && enumFeatures != null && model.DataColumns.Count < enumFeatures.Count)
                {
                    var lackLabels = enumFeatures.Except(model.DataColumns.Select(f => f.Label));
                    var insertModels = lackLabels.Select(l => new DataChartModel()
                    {
                        Label = l,
                        Value = 0
                    }).ToList();

                    var dataColumnsTerm = model.DataColumns.ToList();
                    dataColumnsTerm.AddRange(insertModels);
                    model.DataColumns = dataColumnsTerm;
                    model.DataColumns = model.DataColumns.OrderBy(l => l.Label).ToList();
                }
            }

            if (dayOfWeeks.Count > featureAccsessTimeThisWeek.Count)
            {
                var lackLabels = dayOfWeeks.Except(featureAccsessTimeThisWeek.Select(f => f.Label));

                var insertModels = lackLabels.Select(l => new StackBarChartModel()
                {
                    Label = l,
                    DataColumns = enumFeatures?.Select(x => new DataChartModel()
                    {
                        Label = x,
                        Value = 0
                    }).OrderBy(x => x.Label).ToList()
                }).ToList();

                featureAccsessTimeThisWeek.AddRange(insertModels);

                featureAccsessTimeThisWeek.Sort((x, y) =>
                {
                    var i = (!DayOfWeek.Sunday.ToString().Equals(x.Label, StringComparison.Ordinal)) ? (int)(DayOfWeek)Enum.Parse(typeof(DayOfWeek), x.Label ?? "", ignoreCase: true) : NumberDayOfWeek;
                    var j = (!DayOfWeek.Sunday.ToString().Equals(y.Label, StringComparison.Ordinal)) ? (int)(DayOfWeek)Enum.Parse(typeof(DayOfWeek), y.Label ?? "", ignoreCase: true) : NumberDayOfWeek;
                    return i.CompareTo(j);
                });
            }

            double totalAccsessTimeLastWeek = featureAccsessTime
                .Where(f => f.LastVisited.HasValue &&
                startCalculateDay.AddDays(-NumberDayOfWeek) <= f.LastVisited.Value.Date && DateTime.UtcNow.Date.AddDays(-NumberDayOfWeek) >= f.LastVisited.Value.Date
                ).Sum(f => f.AccessTime);

            totalAccsessTimeLastWeek = totalAccsessTimeLastWeek / totalDays;

            resultModel.DataCharts = featureAccsessTimeThisWeek;
            resultModel.TotalTime = (int)totalTimeThisWeek;

            // Trung bình trên ngày
            resultModel.AveragePerDay = Math.Round(totalTimeThisWeek / totalDays, 0, MidpointRounding.AwayFromZero);

            // Tính phần trăm so với tuần trước
            // Cơ chế giải thích ở dười
            resultModel.Percent = totalAccsessTimeLastWeek == 0 ? 100 : (int)(((resultModel.AveragePerDay - totalAccsessTimeLastWeek) / totalAccsessTimeLastWeek) * 100);

            return resultModel;
        }

        /// <summary>
        /// Hàm lấy dữ liệu học sinh không truy cập vào hệ thống để học tập
        /// </summary>
        /// <param name="student">Danh sách học sinh thuộc trường học</param>
        /// <param name="featureAccsessTime">Dữ liệu truy cập hệ thống của học sinh</param>
        /// <param name="request">Dữ liệu query</param>
        /// <returns>Model tương ứng</returns>
        private static BaseChartResultModel GetStudentNotAccessModel(IList<StudentModel> student, IList<FeatureAccessTimeModel> featureAccsessTime, GetReportDiligenceResultQuery request, IList<string?>? classes)
        {
            // Khởi tạo Model
            // Nhãn model sẽ là lớp của các học sinh đó
            var resultModel = new BaseChartResultModel()
            {
                Type = EnumChartType.BarChart
            };

            // Xác định khoảng thời gian để lọc dữ liệu accesstime
            // Ví dụ ReportDay là 7 thí sẽ trừ đi 6 ngày để lọc
            var reportDate = DateTime.UtcNow.AddDays(-request.ReportDay + 1).Date;

            // Lọc dữ liệu AccessTime
            var featureAccsessTimeInSomeDays = featureAccsessTime.Where(f => f.LastVisited.HasValue && f.LastVisited.Value.Date >= reportDate);

            // Lọc những student không có bản ghi nào trong dữ liệu AccessTiem đã lọc
            var studentNotAccessInSomeDays = student
                .Where(s => !featureAccsessTimeInSomeDays.Any(f => f.CreatedUserId == s.UserId))
                .Where(s => s.SchoolClass != null)
                .GroupBy(s => s.SchoolClass)
                .OrderBy(x => x.Key)
                .Select(x => new DataChartModel()
                {
                    Label = x.Key,
                    Value = x.Count()
                }).ToList();

            if (classes != null && classes.Count > studentNotAccessInSomeDays.Count)
            {
                var lackLabels = classes.Except(studentNotAccessInSomeDays.Select(f => f.Label));
                var insertModels = lackLabels.Select(l => new DataChartModel()
                {
                    Label = l,
                    Value = 0
                }).ToList();

                studentNotAccessInSomeDays.AddRange(insertModels);
                studentNotAccessInSomeDays = studentNotAccessInSomeDays.OrderBy(l => l.Label).ToList();
            }

            resultModel.DataCharts = studentNotAccessInSomeDays;

            return resultModel;
        }

        /// <summary>
        /// Hàm lấy sô lượng học sinh truy cập vào hệ thống để học tập
        /// </summary>
        /// <param name="featureAccsessTime">Dữ liệu truy cập hệ thống của học sinh</param>
        /// <param name="request">Dữ liệu query</param>
        /// <returns>Model tương ứng</returns>
        private static NumberStudentAccessModel GetStudentAccessModel(IList<FeatureAccessTimeModel> featureAccsessTime, GetReportDiligenceResultQuery request)
        {
            // Khởi tạo Model
            var resultModel = new NumberStudentAccessModel()
            {
                Type = EnumChartType.LineChart
            };

            // Lọc dữ liệu AccessTime
            // Nếu truyền ReportEndDate ==> lấy dữ liệu AccessTime theo tuần
            // Nếu không truyền ReportEndDate ==> lấy dữ liệu AccessTime theo ngày
            // Trường ReportStartDate là bắt buộc phải có
            // Dữ liệu DateTime (LastVisited) lấy ra phải chuyển sang giờ local để khớp với giờ nhận vào (ReportStartDate || ReportEndDate) từ query
            var featureAccsessTimeInSomeDays = featureAccsessTime
                    .Where(f =>
                        f.LastVisited.HasValue &&
                        (request.ReportEndDate.HasValue ?
                        request.ReportStartDate.Date <= f.LastVisited.Value.ConvertTimeFromUtc(EnumCountryKey.Vietnam).Date &&
                        request.ReportEndDate.Value.Date >= f.LastVisited.Value.ConvertTimeFromUtc(EnumCountryKey.Vietnam).Date :
                        request.ReportStartDate.Date == f.LastVisited.Value.ConvertTimeFromUtc(EnumCountryKey.Vietnam).Date));

            // Xử lý đổ dữ liệu vào Model với 2 trường hợp
            if (request.ReportEndDate.HasValue)
            {
                // Lấy khoảng ngày
                var dates = new List<string>();
                for (var date = request.ReportStartDate.Date; date <= request.ReportEndDate.Value.Date; date = date.AddDays(1))
                {
                    dates.Add(date.ToString(CultureInfo.CurrentCulture));
                }

                // Lấy dữ liệu theo tuần
                // Nhãn sẽ là các ngày-tháng-năm trong tuần 
                var studentAccessModelThisWeek = featureAccsessTimeInSomeDays
                    .Select(f => new
                    {
                        Date = f.LastVisited!.Value.ConvertTimeFromUtc(EnumCountryKey.Vietnam).Date, // Chọn ra dữ liệu ngày để gom nhóm dữ liệu
                        UserId = f.CreatedUserId
                    })
                    .GroupBy(x => x.Date).OrderBy(x => x.Key)
                    .Select(x => new DataChartModel()
                    {
                        Label = x.Key.ToString(CultureInfo.CurrentCulture), // Gán nhãn (DateTime)
                        Value = x.DistinctBy(x => x.UserId).Count()
                    }).ToList();

                // Tổng lượt truy cập của tuần muốn lấy
                double totalStudentAccessModelThisWeek = studentAccessModelThisWeek.Sum(x => x.Value);
                // Tính tổng truy cập của tuần trước (trừ đi 7 ngày)
                double totalStudentAccessModelLastWeek = featureAccsessTime
                    .Where(f =>
                        f.LastVisited.HasValue &&
                        (request.ReportStartDate.Date.AddDays(-NumberDayOfWeek) <= f.LastVisited.Value.ConvertTimeFromUtc(EnumCountryKey.Vietnam).Date &&
                        request.ReportEndDate.Value.Date.AddDays(-NumberDayOfWeek) >= f.LastVisited.Value.ConvertTimeFromUtc(EnumCountryKey.Vietnam).Date))
                    .Select(f => new
                    {
                        Date = f.LastVisited!.Value.ConvertTimeFromUtc(EnumCountryKey.Vietnam).Date,
                        UserId = f.CreatedUserId
                    })
                    .GroupBy(x => x.Date)
                    .Select(x => x.DistinctBy(x => x.UserId).Count())
                    .Sum();

                if (studentAccessModelThisWeek.Count < dates.Count)
                {
                    var lackLabels = dates.Except(studentAccessModelThisWeek.Select(f => f.Label));
                    var insertModels = lackLabels.Select(l => new DataChartModel()
                    {
                        Label = l,
                        Value = 0
                    }).ToList();

                    studentAccessModelThisWeek.AddRange(insertModels);
                    studentAccessModelThisWeek.Sort((x, y) =>
                    {
                        var i = x.Label != null ? DateTime.Parse(x.Label, CultureInfo.CurrentCulture) : default;
                        var j = y.Label != null ? DateTime.Parse(y.Label, CultureInfo.CurrentCulture) : default;
                        return i.CompareTo(j);
                    });
                }

                resultModel.DataCharts = studentAccessModelThisWeek;

                // Tính phần trăm tăng hoặc giảm tổng lượt truy cập so với tuần trước
                // So sánh với 0 để tránh trường hợp số chia = 0
                // Nếu số chia = 0 ==> mặc định tăng 100%
                // Số dương ==> tăng & ngược lại
                resultModel.Percent = totalStudentAccessModelLastWeek == 0 ? 100 : (int)(((totalStudentAccessModelThisWeek - totalStudentAccessModelLastWeek) / totalStudentAccessModelLastWeek) * 100);
            }
            else
            {
                // Trường hợp lấy dữ liệu theo ngày
                // Nhãn là các mốc giờ trong ngày
                var studentAccessModelThisDay = featureAccsessTimeInSomeDays
                    .Select(f => new
                    {
                        Hour = AssignHourLabelForAccessTime(f.LastVisited), // Gọi hàm gán nhãn cho mỗi bản ghi
                        UserId = f.CreatedUserId
                    })
                    .GroupBy(x => x.Hour).OrderBy(x => (int)x.Key)
                    .Select(x => new DataChartModel()
                    {
                        Label = x.Key.ToString(),
                        Value = x.DistinctBy(x => x.UserId).Count()
                    }).ToList();

                // Tương tự trường hợp trên
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

                var hours = Enum.GetValues(typeof(EnumHour)).Cast<EnumHour>().Select(day => day.ToString()).ToList();
                if (studentAccessModelThisDay.Count < hours.Count)
                {
                    var lackLabels = hours.Except(studentAccessModelThisDay.Select(f => f.Label));
                    var insertModels = lackLabels.Select(l => new DataChartModel()
                    {
                        Label = l,
                        Value = 0
                    }).ToList();

                    studentAccessModelThisDay.AddRange(insertModels);
                    studentAccessModelThisDay.Sort((x, y) =>
                    {
                        var i = (int)(EnumHour)Enum.Parse(typeof(EnumHour), x.Label ?? "", ignoreCase: true);
                        var j = (int)(EnumHour)Enum.Parse(typeof(EnumHour), y.Label ?? "", ignoreCase: true);
                        return i.CompareTo(j);
                    });
                    studentAccessModelThisDay.ForEach(s =>
                    {
                        var enumHour = (EnumHour)Enum.Parse(typeof(EnumHour), s.Label ?? "", ignoreCase: true);
                        s.Label = enumHour.GetDescription();
                    });
                }

                // Tương tự trường hợp trên
                resultModel.DataCharts = studentAccessModelThisDay;
                resultModel.Percent = totalAccessYesterday == 0 ? 100 : (int)(((totalAccessThisDay - totalAccessYesterday) / totalAccessYesterday) * 100);
            }

            return resultModel;
        }

        /// <summary>
        /// Hàm gán nhãn cho dữ liệu DateTime thuộc khoảng giờ nào
        /// </summary>
        /// <param name="date">Dữ liệu DateTime</param>
        /// <returns>EnumHour</returns>
        private static EnumHour AssignHourLabelForAccessTime(DateTime? date)
        {
            if (!date.HasValue)
            {
                return default;
            }

            date = date.Value.ConvertTimeFromUtc(EnumCountryKey.Vietnam);

            foreach (EnumHour hour in Enum.GetValues(typeof(EnumHour)))
            {
                var index = (int)hour * TimePeriod;
                if (index <= date.Value.Hour && index + TimePeriod > date.Value.Hour)
                {
                    return hour;
                }
                continue;
            }

            return default;
        }
    }
}
