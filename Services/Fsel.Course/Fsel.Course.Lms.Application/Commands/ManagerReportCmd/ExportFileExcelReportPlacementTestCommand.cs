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
    using MediatR;
    using OfficeOpenXml;

    public class ExportFileExcelReportPlacementTestCommand : SearchReportPlacementTestQueryModel, IRequest<MethodResult<Stream>>
    {
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
                ListCourseLevel = request.ListCourseLevel,

                SchoolClass = request.SchoolClass,
                SchoolGrade = request.SchoolGrade,
                EndDate = request.EndDate,
                Keyword = request.Keyword,
                Status = request.Status,
                StartDate = request.StartDate,
                CourseLevel = request.CourseLevel,
                CurrentLevel = request.CurrentLevel,
                SortBy = request.SortBy,
            }, cancellationToken);
            var dataOverallResult = await _mediator.Send(new GetOverallReportPlacementTestQuery
            {
                ListDistrict = request.ListDistrict,
                ListProvince = request.ListProvince,
                ListSchool = request.ListSchool,
                ListSchoolClass = request.ListSchoolClass,
                ListSchoolGrade = request.ListSchoolGrade,
                ListCourseLevel = request.ListCourseLevel,

                SchoolClass = request.SchoolClass,
                SchoolGrade = request.SchoolGrade,
                EndDate = request.EndDate,
                Keyword = request.Keyword,
                Status = request.Status,
                StartDate = request.StartDate,
                CourseLevel = request.CourseLevel,
                CurrentLevel = request.CurrentLevel,
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
            using (ExcelPackage excelPackage = new ExcelPackage(new FileInfo(ResourceSettings.ManagerReportPlacementTestExcel)))
            {
                var excelWorksheet = excelPackage.Workbook.Worksheets[0];
                FillParameterData(excelWorksheet, overallReportPlacementTest, schoolName);
                FillSearchKeyData(excelWorksheet, request);
                FillCourseLevelData(excelWorksheet, overallReportPlacementTest);
                if (placementTestReports != null && placementTestReports.Any())
                {
                    FillPlacementTestData(excelWorksheet, placementTestReports);
                }
                excelPackage.SaveAs(memoryStream);
            }

            memoryStream.Position = 0L;
            return memoryStream;
        }

        private static void FillParameterData(ExcelWorksheet excelWorksheet, OverallReportPlacementTestModel? overallReport, string? schoolName)
        {
            excelWorksheet.Cells["D3"].Value = schoolName;
            excelWorksheet.Cells["D4"].Value = overallReport?.TotalStudent ?? default;
            excelWorksheet.Cells["D5"].Value = overallReport?.TotalPlacementTest ?? default;
            excelWorksheet.Cells["D6"].Value = overallReport?.TotalCompletePlacementTest ?? default;
        }

        private static void FillSearchKeyData(ExcelWorksheet excelWorksheet, ExportFileExcelReportPlacementTestCommand request)
        {
            excelWorksheet.Cells["K1"].Value = GetData(excelWorksheet.Cells["K1"].Value, DateTime.UtcNow.ConvertTimeFromUtc(EnumCountryKey.Vietnam).ToString("dd/MM/yyyy hh:mm tt", CultureInfo.InvariantCulture));
            excelWorksheet.Cells["E3"].Value = GetData(excelWorksheet.Cells["E3"].Value, request.Status?.GetDescription());
            excelWorksheet.Cells["F3"].Value = GetData(excelWorksheet.Cells["F3"].Value, request.ListSchoolGrade ?? request.SchoolGrade);
            excelWorksheet.Cells["G3"].Value = GetData(excelWorksheet.Cells["G3"].Value, request.ListSchoolClass ?? request.SchoolClass);
            excelWorksheet.Cells["H3"].Value = GetData(excelWorksheet.Cells["H3"].Value, request.CurrentLevel);
            excelWorksheet.Cells["I3"].Value = GetData(excelWorksheet.Cells["I3"].Value, request.CourseLevel?.GetDescription() ?? request.ListCourseLevel);
            excelWorksheet.Cells["J3"].Value = GetData(excelWorksheet.Cells["J3"].Value, request.StartDate.HasValue ? request.StartDate.Value.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture) : string.Empty);
            excelWorksheet.Cells["K3"].Value = GetData(excelWorksheet.Cells["K3"].Value, request.EndDate.HasValue ? request.EndDate.Value.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture) : string.Empty);
        }

        private static void FillCourseLevelData(ExcelWorksheet excelWorksheet, OverallReportPlacementTestModel? overallReport)
        {
            var courseLevelProgress = overallReport?.CourseLevelProgresses;
            excelWorksheet.Cells["D7"].Value = GetData(excelWorksheet.Cells["D7"].Value, GetTotalCount(courseLevelProgress, EnumCourseLevel.A1));
            excelWorksheet.Cells["E7"].Value = GetData(excelWorksheet.Cells["E7"].Value, GetTotalCount(courseLevelProgress, EnumCourseLevel.A2));
            excelWorksheet.Cells["F7"].Value = GetData(excelWorksheet.Cells["F7"].Value, GetTotalCount(courseLevelProgress, EnumCourseLevel.B1));
            excelWorksheet.Cells["G7"].Value = GetData(excelWorksheet.Cells["G7"].Value, GetTotalCount(courseLevelProgress, EnumCourseLevel.B1Plus));
            excelWorksheet.Cells["H7"].Value = GetData(excelWorksheet.Cells["H7"].Value, GetTotalCount(courseLevelProgress, EnumCourseLevel.B2));
            excelWorksheet.Cells["I7"].Value = GetData(excelWorksheet.Cells["I7"].Value, GetTotalCount(courseLevelProgress, EnumCourseLevel.C1));
        }

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

        private static string GetData(object data, object? param)
        {
            string objStr = data?.ToString() ?? string.Empty;
            return string.Format(objStr, param);
        }

        private static int GetTotalCount(IList<CourseLevelProgressModel>? courseLevelProgresses, EnumCourseLevel courseLevel)
        {
            return courseLevelProgresses?.FirstOrDefault(x => x.CourseLevel == courseLevel)?.TotalStudent ?? default(int);
        }
    }
}
