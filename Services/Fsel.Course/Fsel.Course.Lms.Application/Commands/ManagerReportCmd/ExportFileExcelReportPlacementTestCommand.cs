// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Commands.ManagerReportCmd
{
    using System.Globalization;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Helpers;
    using Fsel.Course.Domain.Models.EntityModels.ManagerReportModels;
    using Fsel.Course.Domain.Models.QueryModels.ManagerReports;
    using Fsel.Course.Lms.Application.Queries.ManagerReportQuery;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Helpers;
    using MediatR;
    using OfficeOpenXml;

    public class ExportFileExcelReportPlacementTestCommand : SearchReportPlacementTestQueryModel, IRequest<MethodResult<Stream>>
    {
        public ExportFileExcelReportPlacementTestCommand()
        {
            ManagerReportType = EnumManagerReportType.ReportManagerPT;
        }
    }

    public class ExportFileExcelReportPlacementTestCommandHandler : IRequestHandler<ExportFileExcelReportPlacementTestCommand, MethodResult<Stream>>
    {
        private readonly IMediator _mediator;
        private readonly IUserService _userService;

        public ExportFileExcelReportPlacementTestCommandHandler(IMediator mediator, IUserService userService)
        {
            _mediator = mediator;
            _userService = userService;
        }

        public async Task<MethodResult<Stream>> Handle(ExportFileExcelReportPlacementTestCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<Stream>();
            var dataResult = await _mediator.Send(new GetReportPlacementTestsQuery
            {
                ListDistrict = request.ListDistrict,
                ListProvince = request.ListProvince,
                ListSchool = request.ListSchool,
                ListSchoolClass = request.ListSchoolClass,
                ListSchoolGrade = request.ListSchoolGrade,
                ListCompletionStatus = request.ListCompletionStatus,
                ListLearningStatus = request.ListLearningStatus,
                ListOverallScore = request.ListOverallScore,
                IsLearning = request.IsLearning,

                EndDate = request.EndDate,
                Keyword = request.Keyword,
                StartDate = request.StartDate,
                SortBy = request.SortBy,
            }, cancellationToken);
            var dataOverallResult = await _mediator.Send(new GetOverallReportPlacementTestQuery
            {
                ListDistrict = request.ListDistrict,
                ListProvince = request.ListProvince,
                ListSchool = request.ListSchool,
                ListSchoolClass = request.ListSchoolClass,
                ListSchoolGrade = request.ListSchoolGrade,
                ListLearningStatus = request.ListLearningStatus,
                ListCompletionStatus = request.ListCompletionStatus,
                ListOverallScore = request.ListOverallScore,
                IsLearning = request.IsLearning,

                EndDate = request.EndDate,
                Keyword = request.Keyword,
                StartDate = request.StartDate,
            }, cancellationToken);
            var userResult = await _userService.GetUserProfileAsync();
            string schoolName = userResult.Content?.Result?.SchoolName ?? string.Empty;
            methodResult.Result = ExportExcelTemplate(request, dataResult.Result, dataOverallResult.Result, schoolName);
            return methodResult;
        }

        public static Stream ExportExcelTemplate(ExportFileExcelReportPlacementTestCommand request, IList<PlacementTestReportModel>? placementTestReports, OverallReportPlacementTestModel? overallReportPlacementTest, string? schoolName)
        {
            ArgumentNullException.ThrowIfNull(request);
            MemoryStream memoryStream = new MemoryStream();
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using (var templateStream = new FileStream(ResourceSettings.ManagerReportPlacementTestExcel, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (ExcelPackage excelPackage = new ExcelPackage(templateStream))
            {
                var excelWorksheet = excelPackage.Workbook.Worksheets[0];
                FillParameterData(excelWorksheet, overallReportPlacementTest, schoolName);
                FillSearchKeyData(excelWorksheet, request);
                //FillCourseLevelData(excelWorksheet, overallReportPlacementTest);
                if (placementTestReports != null && placementTestReports.Any())
                {
                    FillPlacementTestData(excelWorksheet, placementTestReports);
                }
                excelPackage.SaveAs(memoryStream);
            }

            memoryStream.Position = 0L;
            return memoryStream;
        }

        private static object Format(object template, dynamic? values) => Shared.Helpers.StringHelper.FormatStringWithParam(template, values);

        private static void FillParameterData(ExcelWorksheet excelWorksheet, OverallReportPlacementTestModel? overallReport, string? schoolName)
        {
            excelWorksheet.Cells["D3"].Value = schoolName;
            excelWorksheet.Cells["D4"].Value = overallReport?.TotalStudent ?? default;
            excelWorksheet.Cells["D5"].Value = overallReport?.TotalPlacementTest ?? default;
            excelWorksheet.Cells["D6"].Value = overallReport?.TotalCompletePlacementTest ?? default;
        }

        private static void FillSearchKeyData(ExcelWorksheet excelWorksheet, ExportFileExcelReportPlacementTestCommand request)
        {
            string completionStatusStr = string.Join(",", (request.ListCompletionStatus.ToList<EnumCompletionStatus>() ?? new List<EnumCompletionStatus>()).Select(x => x.GetDescription()));
            excelWorksheet.Cells["K1"].Value = Format(excelWorksheet.Cells["K1"].Value, DateTime.UtcNow.ConvertTimeFromUtc(EnumCountryKey.Vietnam).ToString("dd/MM/yyyy hh:mm tt", CultureInfo.InvariantCulture));
            excelWorksheet.Cells["E3"].Value = Format(excelWorksheet.Cells["E3"].Value, completionStatusStr);
            excelWorksheet.Cells["F3"].Value = Format(excelWorksheet.Cells["F3"].Value, request.ListSchoolGrade ?? string.Empty);
            excelWorksheet.Cells["G3"].Value = Format(excelWorksheet.Cells["G3"].Value, request.ListSchoolClass ?? string.Empty);
            excelWorksheet.Cells["J3"].Value = Format(excelWorksheet.Cells["J3"].Value, request.StartDate.HasValue ? request.StartDate.Value.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture) : string.Empty);
            excelWorksheet.Cells["K3"].Value = Format(excelWorksheet.Cells["K3"].Value, request.EndDate.HasValue ? request.EndDate.Value.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture) : string.Empty);
        }

        //private static void FillCourseLevelData(ExcelWorksheet excelWorksheet, OverallReportPlacementTestModel? overallReport)
        //{
        //    var courseLevelProgress = overallReport?.CourseLevelProgresses;
        //    excelWorksheet.Cells["D7"].Value = Format(excelWorksheet.Cells["D7"].Value, GetTotalCount(courseLevelProgress, EnumCourseLevel.A1));
        //    excelWorksheet.Cells["E7"].Value = Format(excelWorksheet.Cells["E7"].Value, GetTotalCount(courseLevelProgress, EnumCourseLevel.A2));
        //    excelWorksheet.Cells["F7"].Value = Format(excelWorksheet.Cells["F7"].Value, GetTotalCount(courseLevelProgress, EnumCourseLevel.B1));
        //    excelWorksheet.Cells["G7"].Value = Format(excelWorksheet.Cells["G7"].Value, GetTotalCount(courseLevelProgress, EnumCourseLevel.B1Plus));
        //    excelWorksheet.Cells["H7"].Value = Format(excelWorksheet.Cells["H7"].Value, GetTotalCount(courseLevelProgress, EnumCourseLevel.B2));
        //    excelWorksheet.Cells["I7"].Value = Format(excelWorksheet.Cells["I7"].Value, GetTotalCount(courseLevelProgress, EnumCourseLevel.C1));
        //}

        private static void FillPlacementTestData(ExcelWorksheet excelWorksheet, IList<PlacementTestReportModel> placementTestReports)
        {
            int startRow = 9;
            foreach (var item in placementTestReports)
            {
                excelWorksheet.Cells[startRow, 1].Value = item.FullName;
                excelWorksheet.Cells[startRow, 2].Value = item.UserName;
                excelWorksheet.Cells[startRow, 3].Value = item.PhoneNumber;
                excelWorksheet.Cells[startRow, 4].Value = item.Email;
                excelWorksheet.Cells[startRow, 5].Value = item.Birthday?.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);
                excelWorksheet.Cells[startRow, 6].Value = item.SchoolGrade;
                excelWorksheet.Cells[startRow, 7].Value = item.SchoolClass;
                excelWorksheet.Cells[startRow, 8].Value = item.ChooseLevelStr;
                excelWorksheet.Cells[startRow, 9].Value = item.CurrentLevelStr;
                excelWorksheet.Cells[startRow, 10].Value = item.StatusDescription;
                excelWorksheet.Cells[startRow, 11].Value = item.ExpiredPTDate?.ToString("dd/MM/yyyy hh:mm", CultureInfo.InvariantCulture);
                startRow++; // Di chuyển xuống dòng tiếp theo
            }
        }
    }
}
