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
        public ExportFileExcelReportLearningProgressCommand()
        {
            ManagerReportType = EnumManagerReportType.ReportLearningProgress;
        }
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

            var dataResult = await _mediator.Send(new GetReportLearningProgressStudentQuery(request), cancellationToken);
            var dataOverallResult = await _mediator.Send(new GetOverallReportLearningProgressQuery(request), cancellationToken);
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

            using (var templateStream = new FileStream(ResourceSettings.ManagerReportLearningProgressExcel, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (ExcelPackage excelPackage = new ExcelPackage(templateStream))
            {
                var excelWorksheet = excelPackage.Workbook.Worksheets[0];
                excelWorksheet.Cells["G2"].Value = schoolName;
                excelWorksheet.Cells["G3"].Value = overallReportLearningProgress?.TotalStudent;

                if (overallReportLearningProgress != null && overallReportLearningProgress.CourseTypeStudents != null)
                {
                    foreach (var item in overallReportLearningProgress.CourseTypeStudents)
                    {
                        string cell = item.CourseType switch
                        {
                            EnumCourseType.Academic => "G4",
                            EnumCourseType.Ielts => "I4",
                            EnumCourseType.EnglishFoundation => "K4",
                            _ => string.Empty
                        };
                        excelWorksheet.Cells[cell].Value = Shared.Helpers.StringHelper.FormatStringWithParam(excelWorksheet.Cells[cell].Value, item.TotalStudent);
                    }
                }

                excelWorksheet.Cells["M1"].Value = Format(excelWorksheet.Cells["M1"].Value, DateTime.UtcNow.ConvertTimeFromUtc(EnumCountryKey.Vietnam).ToString("dd/MM/yyyy hh:mm tt", CultureInfo.InvariantCulture));
                excelWorksheet.Cells["L2"].Value = Format(excelWorksheet.Cells["L2"].Value, request.EndDate.HasValue ? request.EndDate.Value.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture) : string.Empty);
                excelWorksheet.Cells["I2"].Value = Format(excelWorksheet.Cells["I2"].Value, request.ListLearningStatus ?? string.Empty);
                excelWorksheet.Cells["J2"].Value = Format(excelWorksheet.Cells["J2"].Value, request.ListSchoolGrade ?? string.Empty);
                excelWorksheet.Cells["K2"].Value = Format(excelWorksheet.Cells["K2"].Value, request.ListSchoolClass ?? string.Empty);

                //FillCourseLevelData(excelWorksheet, request.CourseType.GetValueOrDefault(), overallReportLearningProgress);
                if (learningProgressReports != null && learningProgressReports.Any())
                {
                    FillLearningProgressData(excelWorksheet, learningProgressReports);
                }
                excelPackage.SaveAs(memoryStream);
            }

            memoryStream.Position = 0L;
            return memoryStream;
        }

        private static object Format(object template, string? values) => Shared.Helpers.StringHelper.FormatStringWithParam(template, values);

        private static void FillLearningProgressData(ExcelWorksheet worksheet, IList<LearningProgressModel> learningProgressReports)
        {
            int startRow = 6;
            int index = 1;
            foreach (var item in learningProgressReports)
            {
                worksheet.Cells[startRow, 1].Value = index++; // STT
                worksheet.Cells[startRow, 2].Value = item.FullName;
                worksheet.Cells[startRow, 3].Value = item.UserName;
                worksheet.Cells[startRow, 4].Value = item.PhoneNumber;
                worksheet.Cells[startRow, 5].Value = item.Email;
                worksheet.Cells[startRow, 6].Value = item.SchoolName;
                worksheet.Cells[startRow, 7].Value = item.SchoolGrade;
                worksheet.Cells[startRow, 8].Value = item.SchoolClass;
                worksheet.Cells[startRow, 9].Value = item.CourseLevel?.GetDescription();
                worksheet.Cells[startRow, 10].Value = item.ContentProgress;
                worksheet.Cells[startRow, 11].Value = item.UnitName;
                worksheet.Cells[startRow, 12].Value = item.LessonName;
                worksheet.Cells[startRow, 13].Value = item.Status.GetDescription();
                startRow++;
            }
        }

        #region
        //public static Stream ExportExcelTemplate(ExportFileExcelReportLearningProgressCommand request, IList<LearningProgressModel>? learningProgressReports, OverallReportLearningProgressModel? overallReportLearningProgress, string? schoolName)
        //{
        //    ArgumentNullException.ThrowIfNull(request);
        //    MemoryStream memoryStream = new MemoryStream();

        //    ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        //    using (ExcelPackage excelPackage = new ExcelPackage(new FileInfo(request.CourseType == EnumCourseType.Academic ? ResourceSettings.ManagerReportLearningProgressAcaExcel : ResourceSettings.ManagerReportLearningProgressIELTSExcel)))
        //    {
        //        var excelWorksheet = excelPackage.Workbook.Worksheets[0];
        //        if (request.CourseType == EnumCourseType.Academic)
        //        {
        //            excelWorksheet.Cells["F2"].Value = schoolName;
        //            excelWorksheet.Cells["F3"].Value = overallReportLearningProgress?.TotalStudent;
        //            excelWorksheet.Cells["F5"].Value = overallReportLearningProgress?.ContentAverageProgress;
        //            excelWorksheet.Cells["K1"].Value = GetData(excelWorksheet.Cells["K1"].Value, DateTime.UtcNow.ConvertTimeFromUtc(EnumCountryKey.Vietnam).ToString("dd/MM/yyyy hh:mm tt", CultureInfo.InvariantCulture));
        //            excelWorksheet.Cells["K2"].Value = GetData(excelWorksheet.Cells["K2"].Value, request.EndDate.HasValue ? request.EndDate.Value.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture) : string.Empty);
        //            excelWorksheet.Cells["G2"].Value = GetData(excelWorksheet.Cells["G2"].Value, request.LearningStatus.HasValue ? request.LearningStatus.Value.GetDescription() : null);
        //            excelWorksheet.Cells["H2"].Value = GetData(excelWorksheet.Cells["H2"].Value, request.ListSchoolGrade ?? request.SchoolGrade);
        //            excelWorksheet.Cells["I2"].Value = GetData(excelWorksheet.Cells["I2"].Value, request.ListSchoolClass ?? request.SchoolClass);
        //            excelWorksheet.Cells["J2"].Value = GetData(excelWorksheet.Cells["J2"].Value, request.CourseLevel);
        //        }
        //        else
        //        {
        //            excelWorksheet.Cells["E2"].Value = schoolName;
        //            excelWorksheet.Cells["E3"].Value = overallReportLearningProgress?.TotalStudent;
        //            excelWorksheet.Cells["E5"].Value = overallReportLearningProgress?.ContentAverageProgress;
        //            excelWorksheet.Cells["K1"].Value = GetData(excelWorksheet.Cells["K1"].Value, DateTime.UtcNow.ConvertTimeFromUtc(EnumCountryKey.Vietnam).ToString("dd/MM/yyyy hh:mm tt", CultureInfo.InvariantCulture));
        //            excelWorksheet.Cells["F2"].Value = GetData(excelWorksheet.Cells["F2"].Value, request.LearningStatus.HasValue ? request.LearningStatus.Value.GetDescription() : null);
        //            excelWorksheet.Cells["G2"].Value = GetData(excelWorksheet.Cells["G2"].Value, request.ListSchoolGrade ?? request.SchoolGrade);
        //            excelWorksheet.Cells["H2"].Value = GetData(excelWorksheet.Cells["H2"].Value, request.ListSchoolClass ?? request.SchoolClass);
        //            excelWorksheet.Cells["I2"].Value = GetData(excelWorksheet.Cells["I2"].Value, request.CourseLevel);
        //            excelWorksheet.Cells["J2"].Value = GetData(excelWorksheet.Cells["J2"].Value, request.EndDate.HasValue ? request.EndDate.Value.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture) : string.Empty);
        //        }

        //        FillCourseLevelData(excelWorksheet, request.CourseType.GetValueOrDefault(), overallReportLearningProgress);
        //        if (learningProgressReports != null && learningProgressReports.Any())
        //        {
        //            FillLearningProgressData(excelWorksheet, learningProgressReports);
        //        }
        //        excelPackage.SaveAs(memoryStream);
        //    }

        //    memoryStream.Position = 0L;
        //    return memoryStream;
        //}

        //private static void FillCourseLevelData(ExcelWorksheet worksheet, EnumCourseType courseType, OverallReportLearningProgressModel? overallReport)
        //{
        //    var courseLevelProgress = overallReport?.CourseLevelProgresses;
        //    if (courseType == EnumCourseType.Academic)
        //    {
        //        worksheet.Cells["F4"].Value = GetData(worksheet.Cells["F4"].Value, GetTotalCount(courseLevelProgress, EnumCourseLevel.A1));
        //        worksheet.Cells["G4"].Value = GetData(worksheet.Cells["G4"].Value, GetTotalCount(courseLevelProgress, EnumCourseLevel.A2));
        //        worksheet.Cells["H4"].Value = GetData(worksheet.Cells["H4"].Value, GetTotalCount(courseLevelProgress, EnumCourseLevel.B1));
        //        worksheet.Cells["I4"].Value = GetData(worksheet.Cells["I4"].Value, GetTotalCount(courseLevelProgress, EnumCourseLevel.B1Plus));
        //        worksheet.Cells["J4"].Value = GetData(worksheet.Cells["J4"].Value, GetTotalCount(courseLevelProgress, EnumCourseLevel.B2));
        //        worksheet.Cells["K4"].Value = GetData(worksheet.Cells["K4"].Value, GetTotalCount(courseLevelProgress, EnumCourseLevel.C1));
        //    }
        //    else
        //    {
        //        worksheet.Cells["E4"].Value = GetData(worksheet.Cells["E4"].Value, GetTotalCount(courseLevelProgress, EnumCourseLevel.MS1));
        //        worksheet.Cells["F4"].Value = GetData(worksheet.Cells["F4"].Value, GetTotalCount(courseLevelProgress, EnumCourseLevel.MS2));
        //        worksheet.Cells["G4"].Value = GetData(worksheet.Cells["G4"].Value, GetTotalCount(courseLevelProgress, EnumCourseLevel.MS3));
        //    }
        //}

        //private static void FillCourseLevelData(ExcelWorksheet worksheet, EnumCourseType courseType, OverallReportLearningProgressModel? overallReport)
        //{
        //    var courseLevelProgress = overallReport?.CourseLevelProgresses;
        //    if (courseType == EnumCourseType.Academic)
        //    {
        //        worksheet.Cells["F4"].Value = GetData(worksheet.Cells["F4"].Value, GetTotalCount(courseLevelProgress, EnumCourseLevel.A1));
        //        worksheet.Cells["G4"].Value = GetData(worksheet.Cells["G4"].Value, GetTotalCount(courseLevelProgress, EnumCourseLevel.A2));
        //        worksheet.Cells["H4"].Value = GetData(worksheet.Cells["H4"].Value, GetTotalCount(courseLevelProgress, EnumCourseLevel.B1));
        //        worksheet.Cells["I4"].Value = GetData(worksheet.Cells["I4"].Value, GetTotalCount(courseLevelProgress, EnumCourseLevel.B1Plus));
        //        worksheet.Cells["J4"].Value = GetData(worksheet.Cells["J4"].Value, GetTotalCount(courseLevelProgress, EnumCourseLevel.B2));
        //        worksheet.Cells["K4"].Value = GetData(worksheet.Cells["K4"].Value, GetTotalCount(courseLevelProgress, EnumCourseLevel.C1));
        //    }
        //    else
        //    {
        //        worksheet.Cells["E4"].Value = GetData(worksheet.Cells["E4"].Value, GetTotalCount(courseLevelProgress, EnumCourseLevel.MS1));
        //        worksheet.Cells["F4"].Value = GetData(worksheet.Cells["F4"].Value, GetTotalCount(courseLevelProgress, EnumCourseLevel.MS2));
        //        worksheet.Cells["G4"].Value = GetData(worksheet.Cells["G4"].Value, GetTotalCount(courseLevelProgress, EnumCourseLevel.MS3));
        //    }
        //}

        //private static int GetTotalCount(IList<CourseLevelProgressModel>? courseLevelProgresses, EnumCourseLevel courseLevel)
        //{
        //    return courseLevelProgresses?.FirstOrDefault(x => x.CourseLevel == courseLevel)?.TotalStudent ?? default(int);
        //}
        #endregion
    }
}
