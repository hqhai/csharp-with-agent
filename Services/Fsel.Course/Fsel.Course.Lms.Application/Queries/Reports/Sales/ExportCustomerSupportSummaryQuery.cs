// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.Reports.Sales
{
    using System.Drawing;
    using System.Globalization;
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
    using Fsel.Course.Lms.Application.Services.SystemService;
    using Fsel.Course.Lms.Application.Services.SystemService.Models;
    using Fsel.Course.Lms.Application.Services.SystemService.QueryModels;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Course.Lms.Application.Services.UserServices.Models;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Helpers;
    using MediatR;
    using Microsoft.EntityFrameworkCore;
    using OfficeOpenXml;
    using OfficeOpenXml.Style;

    public class ExportCustomerSupportSummaryQuery : IRequest<MethodResult<Stream>>
    {
    }

    public class ExportCustomerSupportSummaryQueryHandler : IRequestHandler<ExportCustomerSupportSummaryQuery, MethodResult<Stream>>
    {
        private readonly IOrderService _orderService;
        private readonly IUserService _userService;
        private readonly ISystemService _systemService;
        private readonly IPlacementTestGroupResultRepository _placementTestGroupResultRepository;
        private readonly ICourseResultRepository _courseResultRepository;
        private readonly ManagerProgressHelper _managerProgressHelper;
        private readonly ILessonResultRepository _lessonResultRepository;
        private readonly IUnitResultRepository _unitResultRepository;
        private const double DaysPerWeek = 7.0;
        private const int RequiredLessonsPerWeek = 2;
        private const string StatusAchieved = "Đạt";
        private const string StatusExceeded = "Vượt";
        private const string StatusNotAchieved = "Chưa đạt";

        public ExportCustomerSupportSummaryQueryHandler(IOrderService orderService,
            IUserService userService,
            ISystemService systemService,
            IPlacementTestGroupResultRepository placementTestGroupResultRepository,
            ICourseResultRepository courseResultRepository,
            ManagerProgressHelper managerProgressHelper,
            ILessonResultRepository lessonResultRepository,
            IUnitResultRepository unitResultRepository)
        {
            _orderService = orderService;
            _userService = userService;
            _systemService = systemService;
            _placementTestGroupResultRepository = placementTestGroupResultRepository;
            _courseResultRepository = courseResultRepository;
            _managerProgressHelper = managerProgressHelper;
            _lessonResultRepository = lessonResultRepository;
            _unitResultRepository = unitResultRepository;
        }

        public async Task<MethodResult<Stream>> Handle(ExportCustomerSupportSummaryQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<Stream>();

            var orders = await GetLatestOrdersByUserAsync();
            var userIds = orders.Select(x => x.Key).ToList();

            var featureAccessTimeLasts = await GetFeatureAccessTimesAsync(userIds);
            var featureAccessTimeDicts = featureAccessTimeLasts.ToDictionary(x => x.CreatedUserId);
            var today = DateTime.UtcNow.Date;
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

            var studentReports = new List<StudentLearningReportModel>();
            foreach (var item in students)
            {
                var userId = item.Human?.UserId ?? default;
                courseResultDict.TryGetValue(item.Id, out var courseResult);

                courseCompleteDict.TryGetValue(item.Id, out var courseComplete);
                placementTestGroupResults.TryGetValue(item.Id, out var placementTestGroupResult);
                orders.TryGetValue(userId, out var order);
                featureAccessTimeDicts.TryGetValue(userId, out var featureAccessTime);
                daysSinceLastAccessByUser.TryGetValue(userId, out var daysSinceLast);
                lessonResultGroups.TryGetValue(item.Id, out var countDone);
                unitResultGroups.TryGetValue(item.Id, out var unitResult);

                var studentReport = new StudentLearningReportModel
                {
                    FullName = item.Human?.FullName,
                    Email = item.Human?.Email,
                    SchoolClass = item.SchoolClass,
                    NotLoggedIn = featureAccessTime == null && placementTestGroupResult == null,
                    LoggedInButNoPT = featureAccessTime != null && (placementTestGroupResult == null || placementTestGroupResult.Status != EnumResultStatus.Done),
                    PTButNotStudied = placementTestGroupResult != null && placementTestGroupResult.Status == EnumResultStatus.Done && !placementTestGroupResult.ChooseLevel.HasValue && courseResult == null,
                    SelectedLessonButNotStudied = placementTestGroupResult != null && placementTestGroupResult.ChooseLevel.HasValue && courseResult == null,
                    CurrentLevel = item.CourseLevel,
                    SuggetLevel = placementTestGroupResult?.SuggetLevel,
                    CourseName = courseComplete?.CourseName,
                    PaymentDate = order?.UpdatedDate ?? order?.CreatedDate,
                    FirstStudyDate = courseResult?.ProcessDate,
                    ExpiredDate = item.ExpiredDate,
                    ProgressModule = (item.CourseId.HasValue || courseResult != null) && courseComplete != null ? $"{courseComplete.CountComplete} / {courseComplete.TotalComplete}" : null,
                    ProgressPercent = (item.CourseId.HasValue || courseResult != null) && courseComplete != null ? courseComplete.CountComplete.GetPercent(courseComplete.TotalComplete) : null,
                    DaysSinceLastAccess = daysSinceLast
                };
                var processDate = unitResult?.ProcessDate ?? courseResult?.ProcessDate ?? (courseResult != null && courseResult.Status != EnumResultStatus.New ? courseResult.CreatedDate.AddDays(1) : null);

                if (processDate.HasValue)
                {
                    studentReport.WeeklyResults = CalculateWeeklyProgressList(processDate.Value, today, countDone);
                }
                studentReports.Add(studentReport);
            }

            methodResult.Result = ExportExcelTemplate(studentReports);
            return methodResult;
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
            var studentResults = await _userService.GetUserByIds(userIds);
            return studentResults.Content?.Result ?? new List<StudentModel>();
        }

        private async Task<IList<FeatureAccessTimeModel>> GetFeatureAccessTimesAsync(List<Guid> userIds)
        {
            var result = await _systemService.GetLastFeatureAccessByUserIdsAsync(new GetFeatureAccessTimesByUserIdsQueryModel { UserIds = userIds });
            return result.Content?.Result ?? new List<FeatureAccessTimeModel>();
        }

        private async Task<Dictionary<Guid, int>> GetLessonResultGroupsAsync(List<CourseResultModel> lists, CancellationToken cancellationToken)
        {
            var results = await _lessonResultRepository.Queryable
                .WhereBulkContains(lists.Select(x => new { x.StudentId, x.CourseId }), new[] { "StudentId", "CourseId" })
                .ToListAsync(cancellationToken);
            return results.GroupBy(x => x.StudentId).ToDictionary(x => x.Key, x => x.Count(x => x.Status == EnumResultStatus.Done));
        }

        private async Task<Dictionary<Guid, UnitResult?>> GetUnitResultGroupsAsync(List<CourseResultModel> lists, CancellationToken cancellationToken)
        {
            var results = await _unitResultRepository.Queryable
                .WhereBulkContains(lists.Select(x => new { x.StudentId, x.CourseId }), new[] { "StudentId", "CourseId" })
                .ToListAsync(cancellationToken);
            return results.GroupBy(x => x.StudentId)
                .Select(x => new { StudentId = x.Key, UnitResult = x.OrderBy(y => y.CreatedDate).FirstOrDefault() })
                .ToDictionary(x => x.StudentId, x => x.UnitResult);
        }

        public static Stream ExportExcelTemplate(IList<StudentLearningReportModel>? studentEventLearnProcesses)
        {
            var memoryStream = new MemoryStream();
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using (var stream = File.OpenRead(ResourceSettings.ReportSaleStudentProgress))
            using (var package = new ExcelPackage(stream))
            {
                var maxWeek = studentEventLearnProcesses?.Max(x => x.WeeklyResults?.Count) ?? default;
                ExportTemplateForSheetOne(package.Workbook.Worksheets[0], studentEventLearnProcesses, maxWeek);
                GC.Collect();

                ExportTemplateForSheetTwo(package.Workbook.Worksheets[1], studentEventLearnProcesses, maxWeek);
                GC.Collect();

                ExportTemplateForSheetThree(package.Workbook.Worksheets[2], studentEventLearnProcesses, maxWeek);
                GC.Collect();
                package.SaveAs(memoryStream);
            }

            memoryStream.Position = 0L;
            return memoryStream;
        }

        public static void WriteCellsHorizontalByCountWithRowMerge(ExcelWorksheet worksheet, int startRow, int startColumn, int count, Func<int, string> getValueFunc, bool bold = true, Color? backgroundColor = null)
        {
            ArgumentNullException.ThrowIfNull(worksheet);
            for (int i = 0; i < count; i++)
            {
                var col = startColumn + i;
                var cell = worksheet.Cells[startRow, col, startRow + 1, col];
                cell.Value = getValueFunc(i); // Ví dụ: "Tuần 1", "Tuần 2", ...
                cell.Merge = true;
                cell.Style.Font.Bold = bold;
                cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                cell.Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                if (backgroundColor.HasValue)
                {
                    cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    cell.Style.Fill.BackgroundColor.SetColor(backgroundColor.Value);
                }
                var border = cell.Style.Border;
                border.Top.Style = border.Bottom.Style = border.Left.Style = border.Right.Style = ExcelBorderStyle.Thin;
                border.Top.Color.SetColor(Color.Black);
                border.Bottom.Color.SetColor(Color.Black);
                border.Left.Color.SetColor(Color.Black);
                border.Right.Color.SetColor(Color.Black);
            }
        }

        private static void SetXCentered(ExcelRange cell)
        {
            cell.Value = "X";
            cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
        }

        private static void ExportTemplateForSheetOne(ExcelWorksheet excelWorksheet, IList<StudentLearningReportModel>? studentEventLearnProcesses, int maxWeek)
        {
            WriteCellsHorizontalByCountWithRowMerge(excelWorksheet, startRow: 1, startColumn: 17, count: maxWeek, getValueFunc: i => $"Tuần {i + 1}", backgroundColor: Color.LightGray);

            if (studentEventLearnProcesses != null && studentEventLearnProcesses.Any())
            {
                var startRow = 3;
                foreach (var batch in studentEventLearnProcesses.Chunk(500))
                {
                    foreach (var (item, index) in batch.Select((value, idx) => (value, idx)))
                    {
                        excelWorksheet.Cells[startRow, 1].Value = index + 1;
                        excelWorksheet.Cells[startRow, 2].Value = item.FullName;
                        excelWorksheet.Cells[startRow, 3].Value = item.Email;
                        excelWorksheet.Cells[startRow, 4].Value = item.SchoolClass;
                        if (item.NotLoggedIn)
                        {
                            SetXCentered(excelWorksheet.Cells[startRow, 5]);
                        }
                        if (item.LoggedInButNoPT)
                        {
                            SetXCentered(excelWorksheet.Cells[startRow, 6]);
                        }
                        if (item.PTButNotStudied)
                        {
                            SetXCentered(excelWorksheet.Cells[startRow, 7]);
                        }
                        if (item.SelectedLessonButNotStudied)
                        {
                            SetXCentered(excelWorksheet.Cells[startRow, 8]);
                        }
                        excelWorksheet.Cells[startRow, 9].Value = item.CurrentLevel;
                        excelWorksheet.Cells[startRow, 10].Value = item.SuggetLevel;
                        excelWorksheet.Cells[startRow, 11].Value = item.CourseName;
                        excelWorksheet.Cells[startRow, 13].Value = item.PaymentDate.HasValue ? item.PaymentDate.Value.ConvertTimeFromUtc(EnumCountryKey.Vietnam).ToString("dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture) : null;
                        excelWorksheet.Cells[startRow, 14].Value = item.FirstStudyDate.HasValue ? item.FirstStudyDate.Value.ConvertTimeFromUtc(EnumCountryKey.Vietnam).ToString("dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture) : null;
                        excelWorksheet.Cells[startRow, 15].Value = item.ProgressModule;
                        excelWorksheet.Cells[startRow, 16].Value = item.ProgressPercent.HasValue ? item.ProgressPercent.Value + " %" : null;
                        if (item.WeeklyResults.Any())
                        {
                            var dem = 17;
                            foreach (var weekly in item.WeeklyResults)
                            {
                                var cell = excelWorksheet.Cells[startRow, dem];
                                cell.Value = weekly.Status;
                                cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                                dem++;
                            }
                        }
                        startRow++;
                    }

                    GC.Collect();
                }
            }
        }

        private static void ExportTemplateForSheetTwo(ExcelWorksheet excelWorksheet, IList<StudentLearningReportModel>? studentEventLearnProcesses, int maxWeek)
        {
            WriteCellsHorizontalByCountWithRowMerge(excelWorksheet, startRow: 1, startColumn: 12, count: maxWeek, getValueFunc: i => $"Tuần {i + 1}", backgroundColor: Color.LightGray);
            if (studentEventLearnProcesses != null && studentEventLearnProcesses.Any())
            {
                var startRow = 3;
                foreach (var batch in studentEventLearnProcesses.Chunk(500)) // .NET 6+
                {
                    foreach (var (item, index) in batch.Select((value, idx) => (value, idx)))
                    {
                        excelWorksheet.Cells[startRow, 1].Value = index + 1;
                        excelWorksheet.Cells[startRow, 2].Value = item.FullName;
                        excelWorksheet.Cells[startRow, 3].Value = item.Email;
                        excelWorksheet.Cells[startRow, 4].Value = item.SchoolClass;
                        excelWorksheet.Cells[startRow, 5].Value = item.DaysSinceLastAccess;
                        excelWorksheet.Cells[startRow, 6].Value = item.CurrentLevel;
                        excelWorksheet.Cells[startRow, 7].Value = item.SuggetLevel;
                        excelWorksheet.Cells[startRow, 8].Value = item.CourseName;
                        excelWorksheet.Cells[startRow, 10].Value = item.ProgressModule;
                        excelWorksheet.Cells[startRow, 11].Value = item.ProgressPercent.HasValue ? item.ProgressPercent.Value + " %" : null;
                        if (item.WeeklyResults.Any())
                        {
                            var dem = 12;
                            foreach (var weekly in item.WeeklyResults)
                            {
                                var cell = excelWorksheet.Cells[startRow, dem];
                                cell.Value = weekly.Status;
                                cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                                dem++;
                            }
                        }
                        startRow++;
                    }

                    GC.Collect();
                }
            }
        }

        private static void ExportTemplateForSheetThree(ExcelWorksheet excelWorksheet, IList<StudentLearningReportModel>? studentEventLearnProcesses, int maxWeek)
        {
            WriteCellsHorizontalByCountWithRowMerge(excelWorksheet, startRow: 1, startColumn: 12, count: maxWeek, getValueFunc: i => $"Tuần {i + 1}", backgroundColor: Color.LightGray);
            if (studentEventLearnProcesses != null && studentEventLearnProcesses.Any())
            {
                var startRow = 3;
                foreach (var batch in studentEventLearnProcesses.Chunk(500)) // .NET 6+
                {
                    foreach (var (item, index) in batch.Select((value, idx) => (value, idx)))
                    {
                        excelWorksheet.Cells[startRow, 1].Value = index + 1;
                        excelWorksheet.Cells[startRow, 2].Value = item.FullName;
                        excelWorksheet.Cells[startRow, 3].Value = item.Email;
                        excelWorksheet.Cells[startRow, 4].Value = item.SchoolClass;
                        excelWorksheet.Cells[startRow, 5].Value = item.ExpiredDate.HasValue ? item.ExpiredDate.Value.ConvertTimeFromUtc(EnumCountryKey.Vietnam).ToString("dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture) : null;
                        excelWorksheet.Cells[startRow, 6].Value = item.CurrentLevel;
                        excelWorksheet.Cells[startRow, 7].Value = item.SuggetLevel;
                        excelWorksheet.Cells[startRow, 8].Value = item.CourseName;
                        excelWorksheet.Cells[startRow, 10].Value = item.ProgressModule;
                        excelWorksheet.Cells[startRow, 11].Value = item.ProgressPercent.HasValue ? item.ProgressPercent.Value + " %" : null;
                        if (item.WeeklyResults.Any())
                        {
                            var dem = 12;
                            foreach (var weekly in item.WeeklyResults)
                            {
                                var cell = excelWorksheet.Cells[startRow, dem];
                                cell.Value = weekly.Status;
                                cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                                dem++;
                            }
                        }
                        startRow++;
                    }

                    GC.Collect();
                }
            }
        }

        public static IList<WeeklyProgressModel> CalculateWeeklyProgressList(DateTime startDate, DateTime endDate, int totalCompletedLessons)
        {
            var result = new List<WeeklyProgressModel>();

            int totalDays = (endDate.Date - startDate.Date).Days;
            int totalWeeks = (int)Math.Ceiling(totalDays / DaysPerWeek);

            for (int week = 1; week <= totalWeeks; week++)
            {
                var weekStart = startDate.AddDays((int)((week - 1) * DaysPerWeek));
                var weekEnd = weekStart.AddDays(6);

                string status;
                if (totalCompletedLessons == RequiredLessonsPerWeek)
                {
                    status = StatusAchieved;
                }
                else if (totalCompletedLessons > RequiredLessonsPerWeek)
                {
                    status = StatusExceeded;
                }
                else
                {
                    status = StatusNotAchieved;
                }

                result.Add(new WeeklyProgressModel
                {
                    WeekNumber = week,
                    WeekStartDate = weekStart,
                    WeekEndDate = weekEnd > endDate ? endDate : weekEnd,
                    RequiredLessons = RequiredLessonsPerWeek,
                    CompletedLessons = totalCompletedLessons,
                    Status = status
                });
            }

            return result;
        }
    }
}
