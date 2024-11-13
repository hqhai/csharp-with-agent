// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Commands.ManagerReportCmd
{
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
    using System.Globalization;

    public class ExportFileExcelReportLearningResultCommand : SearchReportLearningResultQueryModel, IRequest<MethodResult<Stream>>
    {
    }

    public class ExportFileExcelReportLearningResultCommandHandler : IRequestHandler<ExportFileExcelReportLearningResultCommand, MethodResult<Stream>>
    {
        private readonly IUserService _userService;
        private readonly IMediator _mediator;

        public ExportFileExcelReportLearningResultCommandHandler(IUserService userService, IMediator mediator)
        {
            _userService = userService;
            _mediator = mediator;
        }

        public async Task<MethodResult<Stream>> Handle(ExportFileExcelReportLearningResultCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<Stream>();
            var dataResult = await _mediator.Send(new GetReportLearningResultQuery()
            {
                ListDistrict = request.ListDistrict,
                ListProvince = request.ListProvince,
                ListSchool = request.ListSchool,
                SchoolGrade = request.SchoolGrade,
                SchoolClass = request.SchoolClass,
                EndDate = request.EndDate,
                Keyword = request.Keyword,
                CourseType = request.CourseType,
                CourseLevel = request.CourseLevel,
                OverallScore = request.OverallScore,
                LearningStatus = request.LearningStatus
            }, cancellationToken);
            var dataOverallResult = await _mediator.Send(new GetOverallReportLearningResultQuery
            {
                ListDistrict = request.ListDistrict,
                ListProvince = request.ListProvince,
                ListSchool = request.ListSchool,
                SchoolGrade = request.SchoolGrade,
                SchoolClass = request.SchoolClass,
                EndDate = request.EndDate,
                Keyword = request.Keyword,
                CourseType = request.CourseType,
                CourseLevel = request.CourseLevel,
                OverallScore = request.OverallScore,
                LearningStatus = request.LearningStatus,
            }, cancellationToken);
            var userResult = await _userService.GetUserProfileAsync();
            string schoolName = userResult.Content?.Result?.SchoolName ?? string.Empty;
            methodResult.Result = ExportExcelTemplate(request, dataResult.Result, dataOverallResult.Result, schoolName);
            return methodResult;
        }

        public static Stream ExportExcelTemplate(ExportFileExcelReportLearningResultCommand request, IList<LearningResultModel>? learningResultReports, OverallReportLearningResultModel? overallReport, string? schoolName)
        {
            ArgumentNullException.ThrowIfNull(request);
            MemoryStream memoryStream = new MemoryStream();
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using (ExcelPackage excelPackage = new ExcelPackage(new FileInfo(request.CourseType == EnumCourseType.Academic ? ResourceSettings.ManagerReportLearningProgressAcaExcel : ResourceSettings.ManagerReportLearningProgressIELTSExcel)))
            {
                var excelWorksheet = excelPackage.Workbook.Worksheets[0];
                excelWorksheet.Cells["E2"].Value = schoolName;
                excelWorksheet.Cells["E3"].Value = overallReport?.TotalStudent;
                excelWorksheet.Cells["E5"].Value = overallReport?.TotalStudent;

                excelWorksheet.Cells["J1"].Value = GetData(excelWorksheet.Cells["J1"].Value, DateTime.UtcNow.ConvertTimeFromUtc(EnumCountryKey.Vietnam).ToString("dd/MM/yyyy hh:mm tt", CultureInfo.InvariantCulture));
                excelWorksheet.Cells["F2"].Value = GetData(excelWorksheet.Cells["F2"].Value, request.LearningStatus.HasValue ? request.LearningStatus.Value.GetDescription() : string.Empty);
                excelWorksheet.Cells["G2"].Value = GetData(excelWorksheet.Cells["G2"].Value, request.SchoolGrade);
                excelWorksheet.Cells["H2"].Value = GetData(excelWorksheet.Cells["H2"].Value, request.SchoolClass);
                excelWorksheet.Cells["I2"].Value = GetData(excelWorksheet.Cells["I2"].Value, request.EndDate.HasValue ? request.EndDate.Value.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture) : string.Empty);

                if (request.CourseType == EnumCourseType.Academic)
                {
                    excelWorksheet.Cells["E4"].Value = GetData(excelWorksheet.Cells["E4"].Value, GetTotalCount(overallReport?.CourseLevelProgresses, EnumCourseLevel.A1));
                    excelWorksheet.Cells["F4"].Value = GetData(excelWorksheet.Cells["F4"].Value, GetTotalCount(overallReport?.CourseLevelProgresses, EnumCourseLevel.A2));
                    excelWorksheet.Cells["G4"].Value = GetData(excelWorksheet.Cells["G4"].Value, GetTotalCount(overallReport?.CourseLevelProgresses, EnumCourseLevel.B1));
                    excelWorksheet.Cells["H4"].Value = GetData(excelWorksheet.Cells["H4"].Value, GetTotalCount(overallReport?.CourseLevelProgresses, EnumCourseLevel.B1Plus));
                    excelWorksheet.Cells["I4"].Value = GetData(excelWorksheet.Cells["I4"].Value, GetTotalCount(overallReport?.CourseLevelProgresses, EnumCourseLevel.B2));
                    excelWorksheet.Cells["J4"].Value = GetData(excelWorksheet.Cells["J4"].Value, GetTotalCount(overallReport?.CourseLevelProgresses, EnumCourseLevel.C1));
                }
                else
                {
                    excelWorksheet.Cells["E4"].Value = GetData(excelWorksheet.Cells["E4"].Value, GetTotalCount(overallReport?.CourseLevelProgresses, EnumCourseLevel.MS1));
                    excelWorksheet.Cells["F4"].Value = GetData(excelWorksheet.Cells["F4"].Value, GetTotalCount(overallReport?.CourseLevelProgresses, EnumCourseLevel.MS2));
                    excelWorksheet.Cells["G4"].Value = GetData(excelWorksheet.Cells["G4"].Value, GetTotalCount(overallReport?.CourseLevelProgresses, EnumCourseLevel.MS3));
                }
                if (learningResultReports != null && learningResultReports.Any())
                {
                    int startRow = 7;
                    foreach (var item in learningResultReports)
                    {
                        excelWorksheet.Cells[startRow, 1].Value = item.FullName;
                        excelWorksheet.Cells[startRow, 2].Value = item.Email;
                        excelWorksheet.Cells[startRow, 3].Value = item.SchoolName;
                        excelWorksheet.Cells[startRow, 4].Value = item.SchoolGrade;
                        excelWorksheet.Cells[startRow, 5].Value = item.SchoolClass;
                        excelWorksheet.Cells[startRow, 6].Value = item.CourseLevel?.GetDescription();
                        foreach (var module in item.OverallModules)
                        {
                            excelWorksheet.Cells[startRow, 6].Value = item.CourseLevel?.GetDescription();
                        }

                        excelWorksheet.Cells[startRow, 10].Value = item.Status.GetDescription();
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

        private static int GetTotalCount(IList<CourseLevelProgressModel>? courseLevelProgresses, EnumCourseLevel courseLevel)
        {
            return courseLevelProgresses?.FirstOrDefault(x => x.CourseLevel == courseLevel)?.TotalStudent ?? default(int);
        }
    }
}
