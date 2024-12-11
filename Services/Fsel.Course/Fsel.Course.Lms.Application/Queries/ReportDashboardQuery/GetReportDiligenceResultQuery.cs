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
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(students));
                return methodResult;
            }

            // Lọc bản ghi có dữ liệu Human
            students = students.Where(s => s.Human != null && s.Human.UserId.HasValue).ToList();

            // Lấy dữ liệu AccessTime
            var accessTimeResult = await _systemService.GetAccessTimeByUserAndFeature(new GetAccessTimeByUserAndFeatureQueryModel
            {
                UserIds = students.Select(s => s.Human!.UserId!.Value).ToList(),
                Features = new List<EnumFeature>() { EnumFeature.ClassForum, EnumFeature.VideoLesson, EnumFeature.HomeWork }
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

            // Gán tên trường học
            resultModel.SchoolName = students.FirstOrDefault()?.School;

            // Lọc thep chương trình học
            if (request.EnumCourseTypes != null && request.EnumCourseTypes.Count > 0)
            {
                students = students.Where(s => request.EnumCourseTypes.Contains(s.CourseLevel.GetEnumCourseType())).ToList();
                featureAccsessTime = featureAccsessTime.Where(f => students.Select(s => s.Human!.UserId).Contains(f.CreatedUserId)).ToList();
            }

            // Lọc theo lớp học của học sinh
            if (request.SchoolClasses != null && request.SchoolClasses.Count > 0)
            {
                students = students.Where(s => s.SchoolClass != null && request.SchoolClasses.Contains(s.SchoolClass)).ToList();
                featureAccsessTime = featureAccsessTime.Where(f => students.Select(s => s.Human!.UserId).Contains(f.CreatedUserId)).ToList();
            }

            resultModel.NumberStudentNotAccessModel = GetStudentNotAccessModel(students, featureAccsessTime, request);

            resultModel.NumberStudentAccessModel = GetStudentAccessModel(featureAccsessTime, request);

            resultModel.LearningResultReportModel = GetLearningResultReportModel(featureAccsessTime);

            methodResult.Result = resultModel;
            methodResult.StatusCode = StatusCodes.Status200OK;

            return methodResult;
        }

        /// <summary>
        /// Lấy thông tin báo cáo kết quả theo tuần
        /// </summary>
        /// <param name="featureAccsessTime">Dữ liệu truy cập hệ thống của học sinh</param>
        /// <returns></returns>
        private static LearningResultReportModel GetLearningResultReportModel(IList<FeatureAccessTimeModel> featureAccsessTime)
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
                    }).ToList()
                });

            double totalAccsessTimeLastWeek = featureAccsessTime
                .Where(f => f.LastVisited.HasValue &&
                startCalculateDay.AddDays(-NumberDayOfWeek) <= f.LastVisited.Value.Date && DateTime.UtcNow.Date.AddDays(-NumberDayOfWeek) >= f.LastVisited.Value.Date
                ).Sum(f => f.AccessTime);

            totalAccsessTimeLastWeek = totalAccsessTimeLastWeek / totalDays;

            resultModel.DataCharts = featureAccsessTimeThisWeek.ToList();
            resultModel.TotalTime = (int)totalTimeThisWeek;

            // Trung bình trên ngày
            resultModel.AveragePerDay = totalTimeThisWeek / totalDays;

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
        private static BaseChartResultModel GetStudentNotAccessModel(IList<StudentModel> student, IList<FeatureAccessTimeModel> featureAccsessTime, GetReportDiligenceResultQuery request)
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
                .Where(s => !featureAccsessTimeInSomeDays.Any(f => f.CreatedUserId == s.Human!.UserId))
                .Where(s => s.SchoolClass != null)
                .GroupBy(s => s.SchoolClass)
                .OrderBy(x => x.Key)
                .Select(x => new DataChartModel()
                {
                    Label = x.Key,
                    Value = x.Count()
                }).ToList();

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
                        Label = x.Key.GetDescription(),
                        Value = x.DistinctBy(x => x.UserId).Count()
                    });

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

                // Tương tự trường hợp trên
                resultModel.DataCharts = studentAccessModelThisDay.ToList();
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
