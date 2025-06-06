// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Commands.ManagerReportCmd
{
    using Fsel.Common.ActionResults;
    using Fsel.Common.Helpers;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Helpers;
    using Fsel.System.Application.Queries.ManagerReportQuery;
    using Fsel.System.Application.Services.UserServices;
    using Fsel.System.Domain.Models.EntityModels.ManagerReportModels;
    using Fsel.System.Domain.Models.QueryModels.ManagerReports;
    using global::System.Globalization;
    using MediatR;
    using OfficeOpenXml;

    public class ExportFileExcelReportStudentAssiduityCommand : SearchStudentReportQueryModel, IRequest<MethodResult<Stream>>
    {
    }

    public class ExportFileExcelReportStudentAssiduityCommandHandler : IRequestHandler<ExportFileExcelReportStudentAssiduityCommand, MethodResult<Stream>>
    {
        private readonly IMediator _mediator;
        private readonly IUserService _userService;

        public ExportFileExcelReportStudentAssiduityCommandHandler(IMediator mediator, IUserService userService)
        {
            _mediator = mediator;
            _userService = userService;
        }

        public async Task<MethodResult<Stream>> Handle(ExportFileExcelReportStudentAssiduityCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<Stream>();
            var dataResult = await _mediator.Send(new GetReportStudentAssiduityQuery
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
                StartDate = request.StartDate,
                CourseType = request.CourseType,
                LearningStatus = request.LearningStatus,
            }, cancellationToken);
            var dataOverallResult = await _mediator.Send(new GetOverallReportStudentAssiduityQuery
            {
                ListDistrict = request.ListDistrict,
                ListProvince = request.ListProvince,
                ListSchool = request.ListSchool,
                ListSchoolClass = request.ListSchoolClass,
                ListSchoolGrade = request.ListSchoolGrade,
                ListCourseLevel = request.ListCourseLevel,

                SchoolGrade = request.SchoolGrade,
                SchoolClass = request.SchoolClass,
                Keyword = request.Keyword,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                LearningStatus = request.LearningStatus,
                CourseType = request.CourseType,
            }, cancellationToken);
            var userResult = await _userService.GetUserProfileAsync();
            string schoolName = userResult.Content?.Result?.SchoolName ?? string.Empty;
            methodResult.Result = ExportExcelTemplate(request, dataResult.Result, dataOverallResult.Result, schoolName);
            return methodResult;
        }

        public static Stream ExportExcelTemplate(ExportFileExcelReportStudentAssiduityCommand request, IList<StudentAssiduityModel>? studentAssiduityReports, OverallReportStudentAssiduityModel? overallReport, string? schoolName)
        {
            ArgumentNullException.ThrowIfNull(request);
            MemoryStream memoryStream = new MemoryStream();
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using (ExcelPackage excelPackage = new ExcelPackage(new FileInfo(ResourceSettings.ManagerReportStudentAssiduityExcel)))
            {
                var excelWorksheet = excelPackage.Workbook.Worksheets[0];
                excelWorksheet.Cells["N2"].Value = GetData(excelWorksheet.Cells["M2"].Value, DateTime.UtcNow.ConvertTimeFromUtc(EnumCountryKey.Vietnam).ToString("dd/MM/yyyy hh:mm tt", CultureInfo.InvariantCulture));
                excelWorksheet.Cells["H2"].Value = GetData(excelWorksheet.Cells["G2"].Value, request.CourseType.HasValue ? request.CourseType.Value : string.Empty);
                excelWorksheet.Cells["I2"].Value = GetData(excelWorksheet.Cells["H2"].Value, request.LearningStatus.HasValue ? request.LearningStatus.Value.GetDescription() : null);
                excelWorksheet.Cells["J2"].Value = GetData(excelWorksheet.Cells["I2"].Value, request.ListSchoolGrade ?? request.SchoolGrade);
                excelWorksheet.Cells["K2"].Value = GetData(excelWorksheet.Cells["J2"].Value, request.ListSchoolClass ?? request.SchoolClass);
                excelWorksheet.Cells["L2"].Value = GetData(excelWorksheet.Cells["K2"].Value, request.StartDate.HasValue ? request.StartDate.Value.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture) : string.Empty);
                excelWorksheet.Cells["M2"].Value = GetData(excelWorksheet.Cells["L2"].Value, request.EndDate.HasValue ? request.EndDate.Value.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture) : string.Empty);

                excelWorksheet.Cells["F2"].Value = schoolName;
                excelWorksheet.Cells["F3"].Value = overallReport?.TotalStudent ?? default;
                excelWorksheet.Cells["F4"].Value = SendMailHelper.FormatTimeSpanAsClock(Shared.Helpers.DateTimeHelper.ConvertSecondsToMinutes((long)(overallReport?.TotalAvgProgressTime ?? default)));
                excelWorksheet.Cells["F5"].Value = overallReport?.TotalAvgVisit ?? default;

                if (studentAssiduityReports != null && studentAssiduityReports.Any())
                {
                    int startRow = 7;
                    foreach (var item in studentAssiduityReports)
                    {
                        excelWorksheet.Cells[startRow, 1].Value = item.FullName;
                        excelWorksheet.Cells[startRow, 2].Value = item.UserName;
                        excelWorksheet.Cells[startRow, 3].Value = item.PhoneNumber;
                        excelWorksheet.Cells[startRow, 4].Value = item.Email;
                        excelWorksheet.Cells[startRow, 5].Value = item.SchoolGrade;
                        excelWorksheet.Cells[startRow, 6].Value = item.SchoolClass;
                        excelWorksheet.Cells[startRow, 7].Value = item.CourseLevel.HasValue ? item.CourseLevel.Value.GetDescription() : null;
                        excelWorksheet.Cells[startRow, 8].Value = item.TotalTimeVideoStr;
                        excelWorksheet.Cells[startRow, 9].Value = item.TotalTimeClassForumStr;
                        excelWorksheet.Cells[startRow, 10].Value = item.TotalTimeHomeWorkStr;
                        excelWorksheet.Cells[startRow, 11].Value = item.TotalTimeStr;
                        excelWorksheet.Cells[startRow, 12].Value = item.TotalVisit;
                        excelWorksheet.Cells[startRow, 13].Value = item.ProcessDate.HasValue ? item.ProcessDate.Value.ConvertTimeFromUtc(EnumCountryKey.Vietnam).ToString("dd/MM/yyyy hh:mm", CultureInfo.InvariantCulture) : null;
                        excelWorksheet.Cells[startRow, 14].Value = item.CurrentDate.HasValue ? item.CurrentDate.Value.ConvertTimeFromUtc(EnumCountryKey.Vietnam).ToString("dd/MM/yyyy hh:mm", CultureInfo.InvariantCulture) : null;
                        excelWorksheet.Cells[startRow, 15].Value = item.ExpiredDate.HasValue ? item.ExpiredDate.Value.ConvertTimeFromUtc(EnumCountryKey.Vietnam).ToString("dd/MM/yyyy hh:mm", CultureInfo.InvariantCulture) : null;
                        startRow++; // Di chuyển xuống dòng tiếp theo
                    }
                }
                excelPackage.SaveAs(memoryStream);
            }

            memoryStream.Position = 0L;
            return memoryStream;
        }

        private static string GetData(object data, object? param)
        {
            string objStr = data?.ToString() ?? string.Empty;
            return string.Format(objStr, param);
        }
    }
}
