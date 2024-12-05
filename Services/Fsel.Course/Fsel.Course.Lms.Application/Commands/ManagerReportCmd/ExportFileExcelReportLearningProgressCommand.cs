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

    public class ExportFileExcelReportLearningProgressCommand : SearchReportLearningProgressQueryModel, IRequest<MethodResult<Stream>>
    {
    }

    public class ExportFileExcelReportLearningProgressCommandHandler : IRequestHandler<ExportFileExcelReportLearningProgressCommand, MethodResult<Stream>>
    {
        private readonly IUserService _userService;
        private readonly IMediator _mediator;

        public ExportFileExcelReportLearningProgressCommandHandler(IUserService userService, IMediator mediator)
        {
            _userService = userService;
            _mediator = mediator;
        }

        public async Task<MethodResult<Stream>> Handle(ExportFileExcelReportLearningProgressCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<Stream>();

            var dataResult = await _mediator.Send(new GetReportLearningProgressStudentQuery()
            {
                ListDistrict = request.ListDistrict,
                ListProvince = request.ListProvince,
                ListSchool = request.ListSchool,
                SchoolGrade = request.SchoolGrade,
                SchoolClass = request.SchoolClass,
                EndDate = request.EndDate,
                Keyword = request.Keyword,
                CourseType = request.CourseType,
                LearningStatus = request.LearningStatus,
                SortBy = request.SortBy,
            }, cancellationToken);
            var dataOverallResult = await _mediator.Send(new GetOverallReportLearningProgressQuery
            {
                ListDistrict = request.ListDistrict,
                ListProvince = request.ListProvince,
                ListSchool = request.ListSchool,
                SchoolGrade = request.SchoolGrade,
                SchoolClass = request.SchoolClass,
                EndDate = request.EndDate,
                Keyword = request.Keyword,
                CourseType = request.CourseType,
                LearningStatus = request.LearningStatus,
            }, cancellationToken);
            var userResult = await _userService.GetUserProfileAsync();
            string schoolName = userResult.Content?.Result?.SchoolName ?? string.Empty;
            methodResult.Result = ExportExcelTemplate(request, dataResult.Result, dataOverallResult.Result, schoolName);
            return methodResult;
        }

        public static Stream ExportExcelTemplate(ExportFileExcelReportLearningProgressCommand request, IList<LearningProgressModel>? learningProgressReports, OverallReportLearningProgressModel? overallReportLearningProgress, string? schoolName)
        {
            ArgumentNullException.ThrowIfNull(request);
            MemoryStream memoryStream = new MemoryStream();

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using (ExcelPackage excelPackage = new ExcelPackage(new FileInfo(request.CourseType == EnumCourseType.Academic ? ResourceSettings.ManagerReportLearningProgressAcaExcel : ResourceSettings.ManagerReportLearningProgressIELTSExcel)))
            {
                var excelWorksheet = excelPackage.Workbook.Worksheets[0];
                excelWorksheet.Cells["D2"].Value = schoolName;
                excelWorksheet.Cells["D3"].Value = overallReportLearningProgress?.TotalStudent;
                excelWorksheet.Cells["D5"].Value = overallReportLearningProgress?.ContentAverageProgress;

                excelWorksheet.Cells["J1"].Value = GetData(excelWorksheet.Cells["J1"].Value, DateTime.UtcNow.ConvertTimeFromUtc(EnumCountryKey.Vietnam).ToString("dd/MM/yyyy hh:mm tt", CultureInfo.InvariantCulture));
                excelWorksheet.Cells["E2"].Value = GetData(excelWorksheet.Cells["E2"].Value, request.LearningStatus.HasValue ? request.LearningStatus.Value.GetDescription() : string.Empty);
                excelWorksheet.Cells["F2"].Value = GetData(excelWorksheet.Cells["F2"].Value, request.SchoolGrade);
                excelWorksheet.Cells["G2"].Value = GetData(excelWorksheet.Cells["G2"].Value, request.SchoolClass);
                excelWorksheet.Cells["H2"].Value = GetData(excelWorksheet.Cells["H2"].Value, request.CourseLevel);
                excelWorksheet.Cells["I2"].Value = GetData(excelWorksheet.Cells["I2"].Value, request.EndDate.HasValue ? request.EndDate.Value.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture) : string.Empty);

                FillCourseLevelData(excelWorksheet, request.CourseType.GetValueOrDefault(), overallReportLearningProgress);
                if (learningProgressReports != null && learningProgressReports.Any())
                {
                    FillLearningProgressData(excelWorksheet, learningProgressReports);
                }
                excelPackage.SaveAs(memoryStream);
            }

            memoryStream.Position = 0L;
            return memoryStream;
        }

        private static void FillCourseLevelData(ExcelWorksheet worksheet, EnumCourseType courseType, OverallReportLearningProgressModel? overallReport)
        {
            var courseLevelProgress = overallReport?.CourseLevelProgresses;
            if (courseType == EnumCourseType.Academic)
            {
                worksheet.Cells["E4"].Value = GetData(worksheet.Cells["E4"].Value, GetTotalCount(courseLevelProgress, EnumCourseLevel.A1));
                worksheet.Cells["F4"].Value = GetData(worksheet.Cells["F4"].Value, GetTotalCount(courseLevelProgress, EnumCourseLevel.A2));
                worksheet.Cells["G4"].Value = GetData(worksheet.Cells["G4"].Value, GetTotalCount(courseLevelProgress, EnumCourseLevel.B1));
                worksheet.Cells["H4"].Value = GetData(worksheet.Cells["H4"].Value, GetTotalCount(courseLevelProgress, EnumCourseLevel.B1Plus));
                worksheet.Cells["I4"].Value = GetData(worksheet.Cells["I4"].Value, GetTotalCount(courseLevelProgress, EnumCourseLevel.B2));
                worksheet.Cells["J4"].Value = GetData(worksheet.Cells["J4"].Value, GetTotalCount(courseLevelProgress, EnumCourseLevel.C1));
            }
            else
            {
                worksheet.Cells["D4"].Value = GetData(worksheet.Cells["D4"].Value, GetTotalCount(courseLevelProgress, EnumCourseLevel.MS1));
                worksheet.Cells["E4"].Value = GetData(worksheet.Cells["E4"].Value, GetTotalCount(courseLevelProgress, EnumCourseLevel.MS2));
                worksheet.Cells["F4"].Value = GetData(worksheet.Cells["F4"].Value, GetTotalCount(courseLevelProgress, EnumCourseLevel.MS3));
            }
        }

        private static void FillLearningProgressData(ExcelWorksheet worksheet, IList<LearningProgressModel> learningProgressReports)
        {
            int startRow = 7;
            foreach (var item in learningProgressReports)
            {
                worksheet.Cells[startRow, 1].Value = item.FullName;
                worksheet.Cells[startRow, 2].Value = item.Email;
                worksheet.Cells[startRow, 3].Value = item.SchoolName;
                worksheet.Cells[startRow, 4].Value = item.SchoolGrade;
                worksheet.Cells[startRow, 5].Value = item.SchoolClass;
                worksheet.Cells[startRow, 6].Value = item.CourseLevel?.GetDescription();
                worksheet.Cells[startRow, 7].Value = item.ContentProgress;
                worksheet.Cells[startRow, 8].Value = item.UnitName;
                worksheet.Cells[startRow, 9].Value = item.LessonName;
                worksheet.Cells[startRow, 10].Value = item.Status.GetDescription();
                startRow++;
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
