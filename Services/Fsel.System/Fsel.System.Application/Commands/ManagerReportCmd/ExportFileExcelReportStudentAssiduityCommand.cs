// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Commands.ManagerReportCmd
{
    using Fsel.Common.ActionResults;
    using Fsel.Common.Helpers;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Enums;
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
                IsLearning = request.IsLearning,
                IsSearchReport = request.IsSearchReport,
                ListCompletionStatus = request.ListCompletionStatus,
                ListLearningStatus = request.ListLearningStatus,

                EndDate = request.EndDate,
                Keyword = request.Keyword,
                StartDate = request.StartDate,
            }, cancellationToken);
            var dataOverallResult = await _mediator.Send(new GetOverallReportStudentAssiduityQuery
            {
                ListDistrict = request.ListDistrict,
                ListProvince = request.ListProvince,
                ListSchool = request.ListSchool,
                ListSchoolClass = request.ListSchoolClass,
                ListSchoolGrade = request.ListSchoolGrade,
                IsLearning = request.IsLearning,
                IsSearchReport = request.IsSearchReport,
                ListCompletionStatus = request.ListCompletionStatus,
                ListLearningStatus = request.ListLearningStatus,

                Keyword = request.Keyword,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
            }, cancellationToken);
            var userResult = await _userService.GetUserProfileAsync();
            string schoolName = userResult.Content?.Result?.SchoolName ?? string.Empty;
            methodResult.Result = ExportExcelTemplate(request, dataResult.Result, dataOverallResult.Result, schoolName);
            return methodResult;
        }

        private static object Format(object template, dynamic? values) => Shared.Helpers.StringHelper.FormatStringWithParam(template, values);

        public static Stream ExportExcelTemplate(ExportFileExcelReportStudentAssiduityCommand request, IList<StudentAssiduityModel>? studentAssiduityReports, OverallReportStudentAssiduityModel? overallReport, string? schoolName)
        {
            ArgumentNullException.ThrowIfNull(request);
            MemoryStream memoryStream = new MemoryStream();
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using (var templateStream = new FileStream(ResourceSettings.ManagerReportStudentAssiduityExcel, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (ExcelPackage excelPackage = new ExcelPackage(templateStream))
            {
                var excelWorksheet = excelPackage.Workbook.Worksheets[0];
                string learningStatuseStr = string.Join(",", (request.ListLearningStatus.ToList<EnumLearningStatus>() ?? new List<EnumLearningStatus>()).Select(x => x.GetDescription()));

                excelWorksheet.Cells["N2"].Value = Format(excelWorksheet.Cells["M2"].Value, DateTime.UtcNow.ConvertTimeFromUtc(EnumCountryKey.Vietnam).ToString("dd/MM/yyyy hh:mm tt", CultureInfo.InvariantCulture));
                excelWorksheet.Cells["I2"].Value = Format(excelWorksheet.Cells["H2"].Value, learningStatuseStr);
                excelWorksheet.Cells["J2"].Value = Format(excelWorksheet.Cells["I2"].Value, request.ListSchoolGrade);
                excelWorksheet.Cells["K2"].Value = Format(excelWorksheet.Cells["J2"].Value, request.ListSchoolClass);
                excelWorksheet.Cells["L2"].Value = Format(excelWorksheet.Cells["K2"].Value, request.StartDate.HasValue ? request.StartDate.Value.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture) : string.Empty);
                excelWorksheet.Cells["M2"].Value = Format(excelWorksheet.Cells["L2"].Value, request.EndDate.HasValue ? request.EndDate.Value.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture) : string.Empty);

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
    }
}
