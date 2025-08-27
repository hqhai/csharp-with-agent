// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.Reports.Sales
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Globalization;
    using System.Linq;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Helpers;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Domain.Models.EntityModels.ExportEventModels;
    using Fsel.Course.Infrastructure.Common;
    using Fsel.Course.Lms.Application.Services.OrderServices;
    using Fsel.Course.Lms.Application.Services.OrderServices.Model;
    using Fsel.Course.Lms.Application.Services.StorageServices;
    using Fsel.Course.Lms.Application.Services.SystemService;
    using Fsel.Course.Lms.Application.Services.SystemService.Models;
    using Fsel.Course.Lms.Application.Services.SystemService.QueryModels;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Course.Lms.Application.Services.UserServices.Models;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Helpers;
    using MassTransit;
    using MediatR;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;
    using OfficeOpenXml;
    using OfficeOpenXml.Style;
    using Refit;

    public class ExportUserInformationSupportSaleQuery : IRequest<MethodResult<Stream>>
    {
        public string? FileName { get; set; }
    }

    public class ExportUserInformationSupportSaleQueryHandler : IRequestHandler<ExportUserInformationSupportSaleQuery, MethodResult<Stream>>
    {
        private readonly IOrderService _orderService;
        private readonly IUserService _userService;
        private readonly ISystemService _systemService;
        private readonly IPlacementTestGroupResultRepository _placementTestGroupResultRepository;
        private readonly ICourseResultRepository _courseResultRepository;
        private readonly ManagerProgressHelper _managerProgressHelper;
        private readonly ILessonResultRepository _lessonResultRepository;
        private readonly IUnitResultRepository _unitResultRepository;
        private readonly IMockTestResultRepository _mockTestResultRepository;
        private readonly IStorageService _storageService;
        private readonly ILogger<ExportUserInformationSupportSaleQueryHandler> _logger;
        private const double DaysPerWeek = 7.0;
        private const int RequiredLessonsPerWeek = 2;
        private const string StatusAchieved = "Đạt";
        private const string StatusExceeded = "Vượt";
        private const string StatusNotAchieved = "Chưa đạt";
        private const int RequiredLessonPerWeek = 1;

        public ExportUserInformationSupportSaleQueryHandler(IOrderService orderService,
            IUserService userService,
            ISystemService systemService,
            IPlacementTestGroupResultRepository placementTestGroupResultRepository,
            ICourseResultRepository courseResultRepository,
            ManagerProgressHelper managerProgressHelper,
            ILessonResultRepository lessonResultRepository,
            IUnitResultRepository unitResultRepository,
            IMockTestResultRepository mockTestResultRepository,
            IStorageService storageService,
            ILogger<ExportUserInformationSupportSaleQueryHandler> logger)
        {
            _orderService = orderService;
            _userService = userService;
            _systemService = systemService;
            _placementTestGroupResultRepository = placementTestGroupResultRepository;
            _courseResultRepository = courseResultRepository;
            _managerProgressHelper = managerProgressHelper;
            _lessonResultRepository = lessonResultRepository;
            _unitResultRepository = unitResultRepository;
            _mockTestResultRepository = mockTestResultRepository;
            _storageService = storageService;
            _logger = logger;
        }

        public async Task<MethodResult<Stream>> Handle(ExportUserInformationSupportSaleQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<Stream>();

            var orders = await GetLatestOrdersByUserAsync();
            var userIds = orders.Select(x => x.Key).ToList();

            var featureAccessTimeLasts = await GetFeatureAccessTimesAsync(userIds);
            var featureAccessTimeDicts = featureAccessTimeLasts.ToDictionary(x => x.CreatedUserId);
            var today = DateTime.UtcNow;
            var daysSinceLastAccessByUser = featureAccessTimeLasts.ToDictionary(
                   x => x.CreatedUserId,
                   x => (today - (x.LastVisited ?? x.UpdatedDate ?? x.CreatedDate ?? today).Date).Days
             );

            var students = await GetStudentsAsync(userIds);

            var placementTestGroupResults = await _placementTestGroupResultRepository.Queryable.WhereBulkContains(students.Select(x => x.Id), x => x.StudentId)
                                                                                     .ToDictionaryAsync(x => x.StudentId, cancellationToken);

            var courseResults = await _courseResultRepository.Queryable.WhereBulkContains(students.Select(x => x.Id), x => x.StudentId)
                                                             .Where(x => x.WorkingStatus == EnumWorkingStatus.Active).ToListAsync(cancellationToken);
            var courseResultDict = courseResults.GroupBy(x => x.StudentId)
                                      .ToDictionary(x => x.Key, x => x.OrderByDescending(x => x.CorrectTotal).OrderByDescending(y => y.UpdatedDate ?? y.CreatedDate).First());

            var lists = students.Select(student =>
            {
                courseResultDict.TryGetValue(student.Id, out var courseResult);
                return new CourseResultModel
                {
                    CourseId = courseResult?.CourseId ?? student.CourseId.GetValueOrDefault(),
                    StudentId = student.Id
                };
            }).ToList();

            var courseCompletes = await _managerProgressHelper.GetProgressCompleteModuleAsync(lists);
            var courseCompleteDict = courseCompletes.ToDictionary(x => x.StudentId);

            var lessonResultGroups = await GetLessonResultGroupsAsync(lists, cancellationToken);
            var unitResultGroups = await GetUnitResultGroupsAsync(lists, cancellationToken);
            var overallScoreGroups = await GetOverallScoreGroupsAsync(lists, cancellationToken);
            var mockTestResultGroups = await GetMockTestResultGroupsAsync(lists, cancellationToken);
            var courseProcessDateGroups = await GetCourseProcessDateGroupsAsync(lists, cancellationToken);

            var dateNow = DateTime.Now;
            var studentReports = new List<StudentInfoLearningReportModel>();
            foreach (var item in students)
            {
                var userId = item.Human?.UserId ?? default;
                courseResultDict.TryGetValue(item.Id, out var courseResult);

                courseCompleteDict.TryGetValue(item.Id, out var courseComplete);
                placementTestGroupResults.TryGetValue(item.Id, out var placementTestGroupResult);
                orders.TryGetValue(userId, out var order);
                featureAccessTimeDicts.TryGetValue(userId, out var featureAccessTime);
                daysSinceLastAccessByUser.TryGetValue(userId, out var daysSinceLast);
                lessonResultGroups.TryGetValue(item.Id, out var lessonResults);
                unitResultGroups.TryGetValue(item.Id, out var unitResult);
                overallScoreGroups.TryGetValue(item.Id, out var overallScore);
                mockTestResultGroups.TryGetValue(item.Id, out var mockTestResults);
                courseProcessDateGroups.TryGetValue(item.Id, out var courseProcessDate);

                var studentReport = new StudentInfoLearningReportModel
                {
                    UserName = item.Human?.User?.UserName,
                    SuggetLevel = placementTestGroupResult?.SuggetLevel,

                    Target = null,
                    CountCourse = null,
                    EstimatedDate = null,

                    FullName = item.Human?.FullName,
                    PhoneNumber = item.Human?.PhoneNumber,
                    Email = item.Human?.Email,
                    BirthDay = item.Human?.Birthday,

                    ParentEmail = item.ParentEmail,
                    ParentFullName = item.ParentFullName,
                    ParentPhoneNumber = item.ParentPhoneNumber,
                    Data = null,
                    School = item.School,
                    SchoolClass = item.SchoolClass,
                    TearchName = null,
                    District = item.District,
                    Province = item.Province,

                    NotLoggedIn = placementTestGroupResult == null && featureAccessTime == null,
                    LoggedInButNoPT = placementTestGroupResult != null && placementTestGroupResult.Status != EnumResultStatus.Done,
                    PTButNotStudied = placementTestGroupResult != null && placementTestGroupResult.Status == EnumResultStatus.Done && !placementTestGroupResult.ChooseLevel.HasValue && courseResult == null,
                    SelectedLessonButNotStudied = placementTestGroupResult != null && placementTestGroupResult.ChooseLevel.HasValue && courseResult == null,
                    ExpiredDaysPaid = item.ExpiredDate.HasValue && item.ExpiredDate < dateNow ? (dateNow - item.ExpiredDate.Value).Days : ValueSettings.ValueDefault,
                    CurrentLevel = item.CourseLevel,
                    Package = order?.MonthNumber + " Tháng",
                    CountOrder = order?.CountOrder ?? default,
                    StudyStartDate = courseProcessDate,
                    ExpiredDate = item.ExpiredDate,
                    LastVisitDate = featureAccessTime?.LastVisited,
                    CountRemainingDay = item.ExpiredDate.HasValue && item.ExpiredDate > dateNow ? (item.ExpiredDate.Value - dateNow).Days : ValueSettings.ValueDefault,
                    CounTabsentDay = daysSinceLast,
                    TokenUser = item.NumberOfToken,

                    FirstStudyDate = courseResult?.ProcessDate,
                    CourseName = courseComplete?.CourseName,
                    CourseOverall = courseResult?.Status == EnumResultStatus.Done ? (courseResult?.Percent ?? ValueSettings.ValueDefault) : overallScore,
                    DataFMT1 = string.Join(" | ", mockTestResults?.FirstOrDefault()?.SkillScores?.Select(s => $"{s.Skill}: {s.Scores:0.##}") ?? Enumerable.Empty<string>()),
                    DataFMT2 = string.Join(" | ", mockTestResults?.LastOrDefault()?.SkillScores?.Select(s => $"{s.Skill}: {s.Scores:0.##}") ?? Enumerable.Empty<string>()),
                    ProgressModule = (item.CourseId.HasValue || courseResult != null) && courseComplete != null ? $"{courseComplete.CountComplete} / {courseComplete.TotalComplete}" : null,
                    ProgressPercent = (item.CourseId.HasValue || courseResult != null) && courseComplete != null ? courseComplete.CountComplete.GetPercent(courseComplete.TotalComplete) : null,
                };
                var processDate = unitResult?.ProcessDate ?? courseResult?.ProcessDate ?? (courseResult != null && courseResult.Status != EnumResultStatus.New ? courseResult.CreatedDate.AddDays(1) : null);
                if (processDate.HasValue && item.CourseLevel.HasValue)
                {
                    studentReport.WeeklyResults = CalculateWeeklyProgressList(processDate.Value.ConvertTimeFromUtc(EnumCountryKey.Vietnam), dateNow, item.CourseLevel.Value, lessonResults, dateNow > item.ExpiredDate);
                }
                studentReports.Add(studentReport);
            }

            methodResult.Result = ExportExcelTemplate(studentReports);
            _logger.LoggerRequest(new
            {
                FileName = request.FileName,
                TotalStudentOrder = userIds.Count,
                TotalStudent = students.Count,
                StudentExport = studentReports.Count,
            });
            await UploadFileExcel(methodResult.Result, request.FileName);
            return methodResult;
        }

        private async Task UploadFileExcel(Stream stream, string? fileName)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                return;
            }
            var filePart = new StreamPart(stream, fileName, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
            await _storageService.UpLoadFile(EnumFolderType.Files, EnumBucketType.FselPublic, filePart, isAddSuffix: false);
        }

        private async Task<Dictionary<Guid, SearchOrderModel?>> GetLatestOrdersByUserAsync()
        {
            var orderResults = await _orderService.GetOrderRevenuesAsync();
            return (orderResults.Content?.Result?.Items ?? new List<SearchOrderModel>())
                    .GroupBy(x => x.UserId)
                    .Select(x => new { UserId = x.Key, Order = x.OrderByDescending(x => x.ExpiredDate).FirstOrDefault() })
                    .ToDictionary(x => x.UserId, x => x.Order);
        }

        private async Task<IList<StudentModel>> GetStudentsAsync(List<Guid> userIds)
        {
            var studentResults = await _userService.GetStudentExportByIds(userIds);
            return studentResults.Content?.Result ?? new List<StudentModel>();
        }

        private async Task<IList<FeatureAccessTimeModel>> GetFeatureAccessTimesAsync(List<Guid> userIds)
        {
            var result = await _systemService.GetLastFeatureAccessByUserIdsAsync(new GetFeatureAccessTimesByUserIdsQueryModel { UserIds = userIds });
            return result.Content?.Result ?? new List<FeatureAccessTimeModel>();
        }

        private async Task<Dictionary<Guid, List<LessonResult>>> GetLessonResultGroupsAsync(List<CourseResultModel> lists, CancellationToken cancellationToken)
        {
            var results = await _lessonResultRepository.Queryable.AsNoTracking()
                .WhereBulkContains(lists.Select(x => new { x.StudentId, x.CourseId }), new[] { "StudentId", "CourseId" })
                .ToListAsync(cancellationToken);
            return results.GroupBy(x => x.StudentId).ToDictionary(x => x.Key, x => x.Where(x => x.Status == EnumResultStatus.Done).OrderBy(x => x.CreatedDate).ToList());
        }

        private async Task<Dictionary<Guid, DateTime?>> GetCourseProcessDateGroupsAsync(List<CourseResultModel> lists, CancellationToken cancellationToken)
        {
            var results = await _courseResultRepository.Queryable.AsNoTracking()
                .WhereBulkContains(lists.Select(x => x.StudentId), x => x.StudentId)
                .ToListAsync(cancellationToken);
            return results.GroupBy(x => x.StudentId).ToDictionary(x => x.Key, x => x.Where(x => x.ProcessDate.HasValue).Select(x => x.ProcessDate).OrderBy(x => x).FirstOrDefault());
        }

        private async Task<Dictionary<Guid, List<MockTestResult>>> GetMockTestResultGroupsAsync(List<CourseResultModel> lists, CancellationToken cancellationToken)
        {
            var results = await _mockTestResultRepository.Queryable.AsNoTracking().Where(x => x.UnitId == null && x.Status == EnumResultStatus.Done)
                            .WhereBulkContains(lists.Select(x => new { x.StudentId, x.CourseId }), new[] { "StudentId", "CourseId" })
                            .ToListAsync(cancellationToken);
            return results.GroupBy(x => x.StudentId).ToDictionary(x => x.Key, x => x.OrderBy(x => x.CreatedDate).ToList());
        }

        private async Task<Dictionary<Guid, UnitResult?>> GetUnitResultGroupsAsync(List<CourseResultModel> lists, CancellationToken cancellationToken)
        {
            var results = await _unitResultRepository.Queryable.AsNoTracking()
                            .WhereBulkContains(lists.Select(x => new { x.StudentId, x.CourseId }), new[] { "StudentId", "CourseId" })
                            .ToListAsync(cancellationToken);
            return results.GroupBy(x => x.StudentId)
                .Select(x => new { StudentId = x.Key, UnitResult = x.OrderBy(y => y.CreatedDate).FirstOrDefault() })
                .ToDictionary(x => x.StudentId, x => x.UnitResult);
        }

        private async Task<Dictionary<Guid, double>> GetOverallScoreGroupsAsync(List<CourseResultModel> lists, CancellationToken cancellationToken)
        {
            var results = await _unitResultRepository.Queryable.AsNoTracking().Where(x => x.Status == EnumResultStatus.Done)
                .WhereBulkContains(lists.Select(x => new { x.StudentId, x.CourseId }), new[] { "StudentId", "CourseId" })
                .ToListAsync(cancellationToken);
            return results.GroupBy(x => x.StudentId)
                .Select(x => new { StudentId = x.Key, Score = NumberHelper.ConvertRound(x.Average(x => x.Percent)) })
                .ToDictionary(x => x.StudentId, x => x.Score);
        }

        public static Stream ExportExcelTemplate(IList<StudentInfoLearningReportModel>? studentEventLearnProcesses)
        {
            var memoryStream = new MemoryStream();
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using (var stream = File.OpenRead(ResourceSettings.ReportSaleStudentProgress))
            using (var package = new ExcelPackage(stream))
            {
                var maxWeek = studentEventLearnProcesses?.Max(x => x.WeeklyResults?.Count) ?? default;
                ExportTemplateForSheetOne(package.Workbook.Worksheets[0], studentEventLearnProcesses, maxWeek);
                GC.Collect();

                ExportTemplateForSheetTwo(package.Workbook.Worksheets[1], studentEventLearnProcesses);
                GC.Collect();

                package.SaveAs(memoryStream);
            }

            memoryStream.Position = 0L;
            return memoryStream;
        }

        public static void WriteCellsHorizontalByCountWithRowMerge(
            ExcelWorksheet ws,
            int startRow,            // hàng bắt đầu của header "Tuần n"
            int startCol,            // cột bắt đầu (VD: cột AN -> số cột tương ứng)
            int count,               // số block tuần cần vẽ
            Func<int, string> getWeekTitle,  // i => $"Tuần {i+1}"
            string[]? subHeaders = null,      // mặc định 3 cột con
            bool bold = true,
            Color? bg = null)
        {
            ArgumentNullException.ThrowIfNull(ws);
            subHeaders ??= new[] { "Status", "Lesson đạt được", "Target tuần" };

            int width = subHeaders.Length; // 3 cột mỗi tuần

            for (int i = 0; i < count; i++)
            {
                int col0 = startCol + i * width;

                // Hàng 1: "Tuần n" gộp ngang 3 cột
                var top = ws.Cells[startRow, col0, startRow + 1, col0 + width - 1];
                top.Merge = true;
                top.Value = getWeekTitle(i);
                top.Style.Font.Bold = bold;
                top.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                top.Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                if (bg.HasValue)
                {
                    top.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    top.Style.Fill.BackgroundColor.SetColor(bg.Value);
                }
                ApplyBorder(top);

                // Hàng 2: 3 header con
                for (int j = 0; j < width; j++)
                {
                    var cell = ws.Cells[startRow + 2, col0 + j];
                    cell.Value = subHeaders[j];
                    cell.Style.Font.Bold = bold;
                    cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    cell.Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                    if (bg.HasValue)
                    {
                        cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        cell.Style.Fill.BackgroundColor.SetColor(Color.Yellow);
                    }
                    ApplyBorder(cell);
                }

                // (tuỳ chọn) set độ rộng cột
                for (int j = 0; j < width; j++)
                    ws.Column(col0 + j).Width = 12; // chỉnh theo ý bạn
            }

            static void ApplyBorder(ExcelRange rng)
            {
                var b = rng.Style.Border;
                b.Top.Style = b.Bottom.Style = b.Left.Style = b.Right.Style = ExcelBorderStyle.Thin;
                b.Top.Color.SetColor(Color.Black);
                b.Bottom.Color.SetColor(Color.Black);
                b.Left.Color.SetColor(Color.Black);
                b.Right.Color.SetColor(Color.Black);
            }
        }

        private static void SetXCentered(ExcelRange cell)
        {
            cell.Value = "X";
            cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
        }

        private static void ExportTemplateForSheetOne(ExcelWorksheet excelWorksheet, IList<StudentInfoLearningReportModel>? studentEventLearnProcesses, int maxWeek)
        {
            WriteCellsHorizontalByCountWithRowMerge(excelWorksheet, startRow: 1, startCol: 40, count: maxWeek, getWeekTitle: i => $"Tuần {i + 1}", bg: Color.LightGray);

            if (studentEventLearnProcesses != null && studentEventLearnProcesses.Any())
            {
                var startRow = 4;
                var toDate = DateTime.Now;
                foreach (var batch in studentEventLearnProcesses.Where(x => x.ExpiredDate > toDate).Chunk(500)) //.Where(x => x.ExpiredDate > toDate)
                {
                    foreach (var (item, index) in batch.Select((value, idx) => (value, idx)))
                    {
                        excelWorksheet.Cells[startRow, 1].Value = index + 1;
                        excelWorksheet.Cells[startRow, 2].Value = item.UserName;
                        excelWorksheet.Cells[startRow, 3].Value = item.SuggetLevel;
                        excelWorksheet.Cells[startRow, 4].Value = item.Target;
                        excelWorksheet.Cells[startRow, 5].Value = item.CountCourse;
                        excelWorksheet.Cells[startRow, 6].Value = item.EstimatedDate?.ConvertTimeFromUtc(EnumCountryKey.Vietnam).ToString("dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture);

                        excelWorksheet.Cells[startRow, 7].Value = item.FullName;
                        excelWorksheet.Cells[startRow, 8].Value = item.PhoneNumber;
                        excelWorksheet.Cells[startRow, 9].Value = item.Email;
                        excelWorksheet.Cells[startRow, 10].Value = item.BirthDay?.ConvertTimeFromUtc(EnumCountryKey.Vietnam).ToString("yyyy", CultureInfo.InvariantCulture);

                        excelWorksheet.Cells[startRow, 11].Value = item.ParentFullName;
                        excelWorksheet.Cells[startRow, 12].Value = item.ParentPhoneNumber;
                        excelWorksheet.Cells[startRow, 13].Value = item.ParentEmail;

                        excelWorksheet.Cells[startRow, 14].Value = item.Data;
                        excelWorksheet.Cells[startRow, 15].Value = item.School;
                        excelWorksheet.Cells[startRow, 16].Value = item.SchoolClass;

                        excelWorksheet.Cells[startRow, 17].Value = item.TearchName;
                        excelWorksheet.Cells[startRow, 18].Value = item.District;
                        excelWorksheet.Cells[startRow, 19].Value = item.Province;
                        if (item.NotLoggedIn)
                        {
                            SetXCentered(excelWorksheet.Cells[startRow, 20]);
                        }
                        if (item.LoggedInButNoPT)
                        {
                            SetXCentered(excelWorksheet.Cells[startRow, 21]);
                        }
                        if (item.PTButNotStudied)
                        {
                            SetXCentered(excelWorksheet.Cells[startRow, 22]);
                        }
                        if (item.SelectedLessonButNotStudied)
                        {
                            SetXCentered(excelWorksheet.Cells[startRow, 23]);
                        }
                        excelWorksheet.Cells[startRow, 24].Value = item.CurrentLevel;
                        excelWorksheet.Cells[startRow, 25].Value = item.Package;
                        excelWorksheet.Cells[startRow, 26].Value = item.CountOrder;
                        excelWorksheet.Cells[startRow, 27].Value = item.StudyStartDate?.ConvertTimeFromUtc(EnumCountryKey.Vietnam).ToString("dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture);
                        excelWorksheet.Cells[startRow, 28].Value = item.ExpiredDate?.ConvertTimeFromUtc(EnumCountryKey.Vietnam).ToString("dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture);
                        excelWorksheet.Cells[startRow, 29].Value = item.LastVisitDate?.ConvertTimeFromUtc(EnumCountryKey.Vietnam).ToString("dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture);
                        excelWorksheet.Cells[startRow, 30].Value = item.CountRemainingDay;
                        excelWorksheet.Cells[startRow, 31].Value = item.CounTabsentDay;
                        excelWorksheet.Cells[startRow, 32].Value = item.TokenUser;
                        excelWorksheet.Cells[startRow, 33].Value = item.FirstStudyDate?.ConvertTimeFromUtc(EnumCountryKey.Vietnam).ToString("dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture);
                        excelWorksheet.Cells[startRow, 34].Value = item.CourseName;
                        excelWorksheet.Cells[startRow, 35].Value = item.ProgressModule;
                        excelWorksheet.Cells[startRow, 36].Value = item.ProgressPercent + " %";
                        excelWorksheet.Cells[startRow, 37].Value = item.DataFMT1;
                        excelWorksheet.Cells[startRow, 38].Value = item.DataFMT2;
                        excelWorksheet.Cells[startRow, 39].Value = item.CourseOverall + " %";
                        if (item.WeeklyResults.Any())
                        {
                            var dem = 40;
                            foreach (var weekly in item.WeeklyResults)
                            {
                                excelWorksheet.Cells[startRow, dem].Value = weekly.Status;
                                dem++;
                                excelWorksheet.Cells[startRow, dem].Value = weekly.LearnedLesson;
                                dem++;
                                excelWorksheet.Cells[startRow, dem].Value = weekly.TargetLesson;
                                dem++;
                            }
                        }
                        startRow++;
                    }

                    GC.Collect();
                }
            }
        }

        private static void ExportTemplateForSheetTwo(ExcelWorksheet excelWorksheet, IList<StudentInfoLearningReportModel>? studentEventLearnProcesses)
        {
            if (studentEventLearnProcesses != null && studentEventLearnProcesses.Any())
            {
                var startRow = 4;
                var toDate = DateTime.Now;
                foreach (var batch in studentEventLearnProcesses.Where(x => x.ExpiredDate < toDate).Chunk(500))
                {
                    foreach (var (item, index) in batch.Select((value, idx) => (value, idx)))
                    {
                        excelWorksheet.Cells[startRow, 1].Value = index + 1;
                        excelWorksheet.Cells[startRow, 2].Value = item.UserName;
                        excelWorksheet.Cells[startRow, 3].Value = item.SuggetLevel;
                        excelWorksheet.Cells[startRow, 4].Value = item.Target;
                        excelWorksheet.Cells[startRow, 5].Value = item.CountCourse;
                        excelWorksheet.Cells[startRow, 6].Value = item.EstimatedDate?.ConvertTimeFromUtc(EnumCountryKey.Vietnam).ToString("dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture);

                        excelWorksheet.Cells[startRow, 7].Value = item.FullName;
                        excelWorksheet.Cells[startRow, 8].Value = item.PhoneNumber;
                        excelWorksheet.Cells[startRow, 9].Value = item.Email;
                        excelWorksheet.Cells[startRow, 10].Value = item.BirthDay?.ConvertTimeFromUtc(EnumCountryKey.Vietnam).ToString("yyyy", CultureInfo.InvariantCulture);

                        excelWorksheet.Cells[startRow, 11].Value = item.ParentFullName;
                        excelWorksheet.Cells[startRow, 12].Value = item.ParentPhoneNumber;
                        excelWorksheet.Cells[startRow, 13].Value = item.ParentEmail;

                        excelWorksheet.Cells[startRow, 14].Value = item.Data;
                        excelWorksheet.Cells[startRow, 15].Value = item.School;
                        excelWorksheet.Cells[startRow, 16].Value = item.SchoolClass;

                        excelWorksheet.Cells[startRow, 17].Value = item.TearchName;
                        excelWorksheet.Cells[startRow, 18].Value = item.District;
                        excelWorksheet.Cells[startRow, 19].Value = item.Province;
                        if (item.NotLoggedIn)
                        {
                            SetXCentered(excelWorksheet.Cells[startRow, 20]);
                        }
                        if (item.LoggedInButNoPT)
                        {
                            SetXCentered(excelWorksheet.Cells[startRow, 21]);
                        }
                        if (item.PTButNotStudied)
                        {
                            SetXCentered(excelWorksheet.Cells[startRow, 22]);
                        }
                        if (item.SelectedLessonButNotStudied)
                        {
                            SetXCentered(excelWorksheet.Cells[startRow, 23]);
                        }
                        excelWorksheet.Cells[startRow, 24].Value = item.ExpiredDaysPaid;
                        excelWorksheet.Cells[startRow, 25].Value = item.CurrentLevel;
                        excelWorksheet.Cells[startRow, 26].Value = item.Package;
                        excelWorksheet.Cells[startRow, 27].Value = item.CountOrder;
                        excelWorksheet.Cells[startRow, 28].Value = item.StudyStartDate?.ConvertTimeFromUtc(EnumCountryKey.Vietnam).ToString("dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture);
                        excelWorksheet.Cells[startRow, 29].Value = item.ExpiredDate?.ConvertTimeFromUtc(EnumCountryKey.Vietnam).ToString("dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture);
                        excelWorksheet.Cells[startRow, 30].Value = item.LastVisitDate?.ConvertTimeFromUtc(EnumCountryKey.Vietnam).ToString("dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture);
                        excelWorksheet.Cells[startRow, 31].Value = item.CountRemainingDay;
                        excelWorksheet.Cells[startRow, 32].Value = item.CounTabsentDay;
                        excelWorksheet.Cells[startRow, 33].Value = item.TokenUser;
                        excelWorksheet.Cells[startRow, 34].Value = item.FirstStudyDate?.ConvertTimeFromUtc(EnumCountryKey.Vietnam).ToString("dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture);
                        excelWorksheet.Cells[startRow, 35].Value = item.CourseName;
                        excelWorksheet.Cells[startRow, 36].Value = item.ProgressModule;
                        excelWorksheet.Cells[startRow, 37].Value = item.ProgressPercent + " %";
                        excelWorksheet.Cells[startRow, 38].Value = item.DataFMT1;
                        excelWorksheet.Cells[startRow, 39].Value = item.DataFMT2;
                        excelWorksheet.Cells[startRow, 40].Value = item.CourseOverall + " %";
                        if (item.WeeklyResults.Any())
                        {
                            var dem = 41;
                            foreach (var weekly in item.WeeklyResults)
                            {
                                excelWorksheet.Cells[startRow, dem].Value = weekly.Status;
                                dem++;
                                excelWorksheet.Cells[startRow, dem].Value = weekly.LearnedLesson;
                                dem++;
                                excelWorksheet.Cells[startRow, dem].Value = weekly.TargetLesson;
                                dem++;
                            }
                        }
                        startRow++;
                    }

                    GC.Collect();
                }
            }
        }

        private static (int lessonsPerUnit, int totalUnits, int lessonsPerLevel) ResolveCoursePlan(EnumCourseLevel level)
        {
            var type = level.GetEnumCourseType();
            int perUnit = type switch { EnumCourseType.Academic => 6, EnumCourseType.Ielts => 4, _ => 5 };
            int units = type switch { EnumCourseType.Academic => 12, EnumCourseType.Ielts => 8, _ => (level == EnumCourseLevel.EFA1 ? 10 : 12) };
            return (perUnit, units, perUnit * units);
        }

        private sealed class LocalLesson
        {
            public int Index { get; init; }
            public LessonResult Lesson { get; init; } = default!;
            public DateTime LocalAt { get; init; }
        }

        // Chuẩn hoá: UTC -> VN 1 lần, lọc trong [s..e] theo Date, sort, rồi gán Index
        private static List<LocalLesson> NormalizeLessons(IList<LessonResult> lessonResults, DateTime s, DateTime e)
        {
            var tz = EnumCountryKey.Vietnam;
            return (lessonResults ?? Array.Empty<LessonResult>())
                .Where(x => x?.UpdatedDate != null)
                .Select(x => new { Lesson = x!, LocalAt = x!.UpdatedDate!.Value.ConvertTimeFromUtc(tz) })
                .Where(x => x.LocalAt >= s && x.LocalAt <= e.Date)
                .OrderBy(x => x.LocalAt)
                .Select((x, idx) => new LocalLesson { Index = idx, Lesson = x.Lesson, LocalAt = x.LocalAt })
                .ToList();
        }

        // Trả về (weekStart, weekEnd) đã kẹp trong [s..e]
        private static (DateTime start, DateTime end) GetWeekRange(DateTime s, int week, DateTime e)
        {
            var start = s.AddDays((week - 1) * DaysPerWeek);
            var end = start.AddDays(6);
            if (end > e)
            {
                end = e;
            }
            return (start, end);
        }

        private static (int lesson, int globalUnit) ToLessonGlobalUnit(int totalLessons, int lessonsPerUnit)
        {
            if (totalLessons <= 0)
            {
                return (0, 1);
            }
            int idx = totalLessons - 1;
            int globalUnit = idx / lessonsPerUnit + 1;
            int lesson = idx % lessonsPerUnit + 1;
            return (lesson, globalUnit);
        }

        private static string FormatProgressCapped(int totalLessons, int lessonsPerLevel, int lessonsPerUnit)
        {
            int capped = Math.Max(0, Math.Min(totalLessons, lessonsPerLevel));
            var (l, gu) = ToLessonGlobalUnit(capped, lessonsPerUnit);
            return $"L{l}U{gu}";
        }

        private static (int learnedCount, string learnedLabel) ComputeLearnedUpTo(List<LocalLesson> lessons, DateTime weekEnd, int lessonsPerLevel, int lessonsPerUnit)
        {
            var last = lessons.LastOrDefault(x => x.LocalAt <= weekEnd);
            int learnedCount = last == null ? 0 : last.Index + 1;
            string learnedLabel = FormatProgressCapped(learnedCount, lessonsPerLevel, lessonsPerUnit);
            return (learnedCount, learnedLabel);
        }

        private static List<LocalLesson> GetWindow(List<LocalLesson> lessons, int windowStartIdx)
        {
            return lessons
                .Where(x => x.Index >= windowStartIdx && x.Index < windowStartIdx + RequiredLessonsPerWeek)
                .ToList();
        }

        private static string ComputeStatus(int completedInWindowByDate)
        {
            if (completedInWindowByDate >= RequiredLessonsPerWeek)
            {
                return StatusExceeded;   // đề phòng dữ liệu trùng
            }
            if (completedInWindowByDate == RequiredLessonPerWeek)
            {
                return StatusAchieved;
            }
            return StatusNotAchieved;
        }

        // ================== MAIN ==================
        public static IList<WeeklyProgressModel> CalculateWeeklyProgressList(
            DateTime startDate,
            DateTime endDate,
            EnumCourseLevel courseLevel,
            IList<LessonResult> lessonResults,
            bool isExpired)
        {
            // Khung báo cáo theo Date (local)
            var s = startDate;
            var e = endDate;
            if (e < s)
            {
                return new List<WeeklyProgressModel>();
            }
            int totalDays = (e - s).Days + 1;
            int totalWeeks = (int)Math.Ceiling(totalDays / DaysPerWeek);

            // Config khóa học
            var plan = ResolveCoursePlan(courseLevel);
            int lessonsPerUnit = plan.lessonsPerUnit;
            int lessonsPerLevel = plan.lessonsPerLevel;
            var lessons = NormalizeLessons(lessonResults, s, e);
            int totalCompletedOverall = Math.Min(lessons.Count, lessonsPerLevel);

            // Present (đóng băng ở cuối level trong phạm vi báo cáo)
            string presentLesson = FormatProgressCapped(totalCompletedOverall, lessonsPerLevel, lessonsPerUnit);
            var result = new List<WeeklyProgressModel>();

            for (int week = 1; week <= totalWeeks; week++)
            {
                var (weekStart, weekEnd) = GetWeekRange(s, week, e);

                // Cửa sổ theo index 2 bài/tuần: [0..1], [2..3], ...
                int windowStartIdx = (week - 1) * RequiredLessonsPerWeek;

                // Lũy kế đến cuối tuần
                var (learnedCount, learnedLabel) = ComputeLearnedUpTo(lessons, weekEnd, lessonsPerLevel, lessonsPerUnit);

                // Cửa sổ kỳ vọng
                var window = GetWindow(lessons, windowStartIdx);

                // Bao nhiêu bài của window đã hoàn thành đến cuối tuần
                int completedInWindowByDate = window.Count(x => x.LocalAt <= weekEnd);

                // Target tuần (lũy kế 2,4,6,... capped theo level)
                int expectedRaw = week * RequiredLessonsPerWeek;
                int expectedCapped = Math.Min(expectedRaw, lessonsPerLevel);
                string targetLesson = FormatProgressCapped(expectedCapped, lessonsPerLevel, lessonsPerUnit);

                // Trạng thái
                string status = ComputeStatus(completedInWindowByDate);

                result.Add(new WeeklyProgressModel
                {
                    WeekNumber = week,
                    WeekStartDate = weekStart,
                    WeekEndDate = weekEnd,

                    PresentLesson = presentLesson,
                    LearnedLesson = learnedLabel,
                    TargetLesson = targetLesson,
                    RequiredLessons = RequiredLessonsPerWeek,

                    CompletedLessons = completedInWindowByDate,
                    Status = status
                });

                // Nếu đã xong level và không còn window tuần này, dừng luôn (tránh 1 vòng lặp rỗng)
                if (window.Count == 0 && learnedCount >= lessonsPerLevel)
                {
                    break;
                }
            }

            return isExpired
                ? result.OrderByDescending(x => x.WeekStartDate).Take(4).ToList()
                : result.OrderBy(x => x.WeekStartDate).ToList();
        }
    }
}
