// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Commands.ManagerReportCmd
{
    using System.Globalization;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Helpers;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.Models.EntityModels.ManagerReportModels;
    using Fsel.Course.Domain.Models.QueryModels.ManagerReports;
    using Fsel.Course.Lms.Application.Queries.ManagerReportQuery;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Enums;
    using MediatR;
    using OfficeOpenXml;
    using static Fsel.Shared.Constants.ValueSettings;

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
                ListSchoolClass = request.ListSchoolClass,
                ListSchoolGrade = request.ListSchoolGrade,
                ListCourseLevel = request.ListCourseLevel,
                ListCompletionStatus = request.ListCompletionStatus,
                ListLearningStatus = request.ListLearningStatus,
                ListOverallScore = request.ListOverallScore,
                IsLearning = request.IsLearning,
                CourseType = request.CourseType,

                EndDate = request.EndDate,
                Keyword = request.Keyword,
                SortBy = request.SortBy,
            }, cancellationToken);
            var dataOverallResult = await _mediator.Send(new GetOverallReportLearningResultQuery
            {
                ListDistrict = request.ListDistrict,
                ListProvince = request.ListProvince,
                ListSchool = request.ListSchool,
                ListSchoolClass = request.ListSchoolClass,
                ListSchoolGrade = request.ListSchoolGrade,
                ListCourseLevel = request.ListCourseLevel,
                ListOverallScore = request.ListOverallScore,
                IsLearning = request.IsLearning,
                ListLearningStatus = request.ListLearningStatus,
                ListCompletionStatus = request.ListCompletionStatus,
                ListCurrentLevel = request.ListCurrentLevel,
                CourseType = request.CourseType,

                EndDate = request.EndDate,
                Keyword = request.Keyword,
            }, cancellationToken);
            var userResult = await _userService.GetUserProfileAsync();
            string schoolName = userResult.Content?.Result?.SchoolName ?? string.Empty;
            methodResult.Result = ExportExcelTemplate(request, dataResult.Result, dataOverallResult.Result, schoolName);
            return methodResult;
        }

        private static string GetTemplateFilePath(EnumCourseType courseType)
        {
            return courseType switch
            {
                EnumCourseType.Academic => ResourceSettings.ManagerReportLearningResultAcaExcel,
                EnumCourseType.Ielts => ResourceSettings.ManagerReportLearningResultIELTSExcel,
                _ => ResourceSettings.ManagerReportLearningResultEFExcel
            };
        }

        public static Stream ExportExcelTemplate(ExportFileExcelReportLearningResultCommand request, IList<LearningResultReportModel>? learningResultReports, OverallReportLearningResultModel? overallReport, string? schoolName)
        {
            ArgumentNullException.ThrowIfNull(request);
            MemoryStream memoryStream = new MemoryStream();
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            string filePath = GetTemplateFilePath(request.CourseType ?? default);

            using (var templateStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (ExcelPackage excelPackage = new ExcelPackage(templateStream))
            {
                var excelWorksheet = excelPackage.Workbook.Worksheets[0];
                string learningStatuseStr = string.Join(",", (request.LearningStatuses ?? new List<EnumLearningStatus>()).Select(x => x.GetDescription()));
                string overallScoreStr = string.Join(",", (request.OverallScores ?? new List<EnumOverallScore>()).Select(x => x.GetDescription()));

                switch (request.CourseType)
                {
                    case EnumCourseType.Ielts:
                        FillIeltsHeader(excelWorksheet, request, overallReport, schoolName, learningStatuseStr, overallScoreStr);
                        break;

                    case EnumCourseType.Academic:
                        FillAcademicHeader(excelWorksheet, request, overallReport, schoolName, learningStatuseStr, overallScoreStr);
                        break;

                    default:
                        FillEfaHeader(excelWorksheet, request, overallReport, schoolName, learningStatuseStr, overallScoreStr);
                        break;
                }

                FillUnitData(excelWorksheet, request.CourseType.GetValueOrDefault(), overallReport);
                FillCourseLevelData(excelWorksheet, request.CourseType.GetValueOrDefault(), overallReport);
                if (learningResultReports != null && learningResultReports.Any())
                {
                    FillLearningProgressData(excelWorksheet, request.CourseType.GetValueOrDefault(), learningResultReports);
                }
                excelPackage.SaveAs(memoryStream);
            }

            memoryStream.Position = 0L;
            return memoryStream;
        }

        private static void FillIeltsHeader(ExcelWorksheet sheet, ExportFileExcelReportLearningResultCommand request, OverallReportLearningResultModel? report, string? schoolName, string statusStr, string scoreStr)
        {
            sheet.Cells["F2"].Value = schoolName;
            sheet.Cells["F3"].Value = report?.TotalStudent;
            sheet.Cells["F7"].Value = ConvertPercent(report?.OverallAvgPercent);

            sheet.Cells["G2"].Value = Format(sheet.Cells["G2"].Value, statusStr);
            sheet.Cells["H2"].Value = Format(sheet.Cells["H2"].Value, request.ListSchoolGrade ?? string.Empty);
            sheet.Cells["I2"].Value = Format(sheet.Cells["I2"].Value, request.ListSchoolClass ?? string.Empty);
            sheet.Cells["J2"].Value = Format(sheet.Cells["J2"].Value, request.ListCourseLevel ?? string.Empty);
            sheet.Cells["K2"].Value = Format(sheet.Cells["K2"].Value, scoreStr);
            sheet.Cells["L2"].Value = Format(sheet.Cells["L2"].Value, request.EndDate?.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture) ?? string.Empty);
            sheet.Cells["M2"].Value = Format(sheet.Cells["M2"].Value, DateTime.UtcNow.ConvertTimeFromUtc(EnumCountryKey.Vietnam).ToString("dd/MM/yyyy  hh:mm tt", CultureInfo.InvariantCulture));
        }

        private static void FillAcademicHeader(ExcelWorksheet sheet, ExportFileExcelReportLearningResultCommand request, OverallReportLearningResultModel? report, string? schoolName, string statusStr, string scoreStr)
        {
            sheet.Cells["G2"].Value = schoolName;
            sheet.Cells["G3"].Value = report?.TotalStudent;
            sheet.Cells["G5"].Value = ConvertPercent(report?.OverallAvgPercent);
            sheet.Cells["G8"].Value = ConvertPercent(report?.OverallAvgPercentFinal);

            sheet.Cells["H2"].Value = Format(sheet.Cells["H2"].Value, statusStr);
            sheet.Cells["I2"].Value = Format(sheet.Cells["I2"].Value, request.ListSchoolGrade ?? string.Empty);
            sheet.Cells["J2"].Value = Format(sheet.Cells["J2"].Value, request.ListSchoolClass ?? string.Empty);
            sheet.Cells["K2"].Value = Format(sheet.Cells["K2"].Value, request.ListCourseLevel ?? string.Empty);
            sheet.Cells["L2"].Value = Format(sheet.Cells["L2"].Value, scoreStr);
            sheet.Cells["M2"].Value = Format(sheet.Cells["M2"].Value, request.EndDate?.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture) ?? string.Empty);
            sheet.Cells["N2"].Value = Format(sheet.Cells["N2"].Value, DateTime.UtcNow.ConvertTimeFromUtc(EnumCountryKey.Vietnam).ToString("dd/MM/yyyy hh:mm tt", CultureInfo.InvariantCulture));
        }

        private static void FillEfaHeader(ExcelWorksheet sheet, ExportFileExcelReportLearningResultCommand request, OverallReportLearningResultModel? report, string? schoolName, string statusStr, string scoreStr)
        {
            sheet.Cells["F2"].Value = schoolName;
            sheet.Cells["F3"].Value = report?.TotalStudent;
            sheet.Cells["F5"].Value = ConvertPercent(report?.OverallAvgPercent);
            sheet.Cells["F8"].Value = ConvertPercent(report?.OverallAvgPercentFinal);

            sheet.Cells["G2"].Value = Format(sheet.Cells["G2"].Value, statusStr);
            sheet.Cells["H2"].Value = Format(sheet.Cells["H2"].Value, request.ListSchoolGrade ?? string.Empty);
            sheet.Cells["I2"].Value = Format(sheet.Cells["I2"].Value, request.ListSchoolClass ?? string.Empty);
            sheet.Cells["J2"].Value = Format(sheet.Cells["J2"].Value, request.ListCourseLevel ?? string.Empty);
            sheet.Cells["K2"].Value = Format(sheet.Cells["K2"].Value, scoreStr);
            sheet.Cells["L2"].Value = Format(sheet.Cells["L2"].Value, request.EndDate?.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture) ?? string.Empty);
            sheet.Cells["M2"].Value = Format(sheet.Cells["M2"].Value, DateTime.UtcNow.ConvertTimeFromUtc(EnumCountryKey.Vietnam).ToString("dd/MM/yyyy hh:mm tt", CultureInfo.InvariantCulture));
        }

        private static object Format(object template, string? values) => Shared.Helpers.StringHelper.FormatStringWithParam(template, values);

        private static object Format(object template, params object[]? values) => Shared.Helpers.StringHelper.FormatStringWithParam(template, values);

        private static string ConvertPercent(double? percent, string pattern = "0 %")
        {
            return percent.HasValue ? $"{percent.Value} %" : pattern;
        }

        private static void FillUnitData(ExcelWorksheet excelWorksheet, EnumCourseType courseType, OverallReportLearningResultModel? overallReport)
        {
            var courseLevelProgress = overallReport?.OverallModules?.Where(x => x.Type == nameof(Domain.Entities.Unit)).OrderBy(x => x.Index).ToList();
            if (courseLevelProgress == null)
            {
                return;
            }
            if (courseType == EnumCourseType.Academic)
            {
                var startRow = 7;
                foreach (var item in courseLevelProgress)
                {
                    excelWorksheet.Cells[6, startRow].Value = Format(excelWorksheet.Cells[6, startRow].Value, ConvertPercent(item.Percent));
                    excelWorksheet.Cells[7, startRow].Value = item.TotalStudent + " hs";
                    startRow++;
                }
            }
            else if (courseType == EnumCourseType.Ielts)
            {
                var startRow = 6;
                foreach (var item in courseLevelProgress)
                {
                    excelWorksheet.Cells[8, startRow].Value = Format(excelWorksheet.Cells[8, startRow].Value, ConvertPercent(item.Percent));
                    excelWorksheet.Cells[9, startRow].Value = item.TotalStudent + " hs";
                    startRow++;
                }
            }
            else
            {
                var startRow = 6;
                foreach (var item in courseLevelProgress)
                {
                    excelWorksheet.Cells[6, startRow].Value = Format(excelWorksheet.Cells[6, startRow].Value, ConvertPercent(item.Percent));
                    excelWorksheet.Cells[7, startRow].Value = item.TotalStudent + " hs";
                    startRow++;
                }
            }
        }

        private static void FillCourseLevelData(ExcelWorksheet excelWorksheet, EnumCourseType courseType, OverallReportLearningResultModel? overallReport)
        {
            var courseLevelProgress = overallReport?.CourseLevelProgresses;
            if (courseType == EnumCourseType.Academic)
            {
                excelWorksheet.Cells["G4"].Value = Format(excelWorksheet.Cells["G4"].Value, GetTotalCount(courseLevelProgress, EnumCourseLevel.A1));
                excelWorksheet.Cells["H4"].Value = Format(excelWorksheet.Cells["H4"].Value, GetTotalCount(courseLevelProgress, EnumCourseLevel.A2));
                excelWorksheet.Cells["I4"].Value = Format(excelWorksheet.Cells["I4"].Value, GetTotalCount(courseLevelProgress, EnumCourseLevel.B1));
                excelWorksheet.Cells["J4"].Value = Format(excelWorksheet.Cells["J4"].Value, GetTotalCount(courseLevelProgress, EnumCourseLevel.B1Plus));
                excelWorksheet.Cells["K4"].Value = Format(excelWorksheet.Cells["K4"].Value, GetTotalCount(courseLevelProgress, EnumCourseLevel.B2));
                excelWorksheet.Cells["L4"].Value = Format(excelWorksheet.Cells["L4"].Value, GetTotalCount(courseLevelProgress, EnumCourseLevel.C1));
            }
            else if (courseType == EnumCourseType.Ielts)
            {
                var ms1Progress = courseLevelProgress?.FirstOrDefault(x => x.CourseLevel == EnumCourseLevel.MS1);
                var ms1ProgressFirst = ms1Progress?.OverallTestResults?.FirstOrDefault();
                var ms1ProgressLast = ms1Progress?.OverallTestResults?.LastOrDefault();
                excelWorksheet.Cells["F4"].Value = Format(excelWorksheet.Cells["F4"].Value, $"{ms1Progress?.TotalStudent ?? default} hs");
                excelWorksheet.Cells["F5"].Value = Format(excelWorksheet.Cells["F5"].Value, new string[] { $"{ms1ProgressFirst?.Score ?? default}", $"{ms1ProgressFirst?.TotalStudent ?? default} hs" });
                excelWorksheet.Cells["F6"].Value = Format(excelWorksheet.Cells["F6"].Value, new string[] { $"{ms1ProgressLast?.Score ?? default}", $"{ms1ProgressLast?.TotalStudent ?? default} hs" });

                var ms2Progress = courseLevelProgress?.FirstOrDefault(x => x.CourseLevel == EnumCourseLevel.MS2);
                var ms2ProgressFirst = ms2Progress?.OverallTestResults?.FirstOrDefault();
                var ms2ProgressLast = ms2Progress?.OverallTestResults?.LastOrDefault();
                excelWorksheet.Cells["G4"].Value = Format(excelWorksheet.Cells["G4"].Value, ms2Progress?.TotalStudent ?? default);
                excelWorksheet.Cells["G5"].Value = Format(excelWorksheet.Cells["G5"].Value, new string[] { $"{ms2ProgressFirst?.Score ?? default}", $"{ms2ProgressFirst?.TotalStudent ?? default} hs" });
                excelWorksheet.Cells["G6"].Value = Format(excelWorksheet.Cells["G6"].Value, new string[] { $"{ms2ProgressLast?.Score ?? default}", $"{ms2ProgressLast?.TotalStudent ?? default} hs" });

                var ms3Progress = courseLevelProgress?.FirstOrDefault(x => x.CourseLevel == EnumCourseLevel.MS3);
                var ms3ProgressFirst = ms3Progress?.OverallTestResults?.FirstOrDefault();
                var ms3ProgressLast = ms3Progress?.OverallTestResults?.LastOrDefault();
                excelWorksheet.Cells["H4"].Value = Format(excelWorksheet.Cells["H4"].Value, ms3Progress?.TotalStudent ?? default);
                excelWorksheet.Cells["H5"].Value = Format(excelWorksheet.Cells["H5"].Value, new string[] { $"{ms3ProgressFirst?.Score ?? default}", $"{ms3ProgressFirst?.TotalStudent ?? default} hs" });
                excelWorksheet.Cells["H6"].Value = Format(excelWorksheet.Cells["H6"].Value, new string[] { $"{ms3ProgressLast?.Score ?? default}", $"{ms3ProgressLast?.TotalStudent ?? default} hs" });
            }
            else
            {
                excelWorksheet.Cells["F4"].Value = Format(excelWorksheet.Cells["F4"].Value, GetTotalCount(courseLevelProgress, EnumCourseLevel.EFA1));
                excelWorksheet.Cells["G4"].Value = Format(excelWorksheet.Cells["G4"].Value, GetTotalCount(courseLevelProgress, EnumCourseLevel.EFA2));
                excelWorksheet.Cells["H4"].Value = Format(excelWorksheet.Cells["H4"].Value, GetTotalCount(courseLevelProgress, EnumCourseLevel.EFB1));
            }
        }

        private static void FillLearningProgressData(ExcelWorksheet worksheet, EnumCourseType courseType, IList<LearningResultReportModel> learningProgressReports)
        {
            int startRow = courseType == EnumCourseType.Ielts ? 11 : 10;

            foreach (var item in learningProgressReports)
            {
                var processDateStr = item.ProcessDate.HasValue ? item.ProcessDate.Value.ConvertTimeFromUtc(EnumCountryKey.Vietnam).ToString("dd/MM/yyyy hh:mm tt", CultureInfo.InvariantCulture) : string.Empty;
                var expiredDateStr = item.ExpiredDate.HasValue ? item.ExpiredDate.Value.ConvertTimeFromUtc(EnumCountryKey.Vietnam).ToString("dd/MM/yyyy hh:mm tt", CultureInfo.InvariantCulture) : string.Empty;

                worksheet.Cells[startRow, 1].Value = item.FullName;
                worksheet.Cells[startRow, 2].Value = item.UserName;
                worksheet.Cells[startRow, 3].Value = item.PhoneNumber;
                worksheet.Cells[startRow, 4].Value = item.Email;
                worksheet.Cells[startRow, 5].Value = item.SchoolGrade;
                worksheet.Cells[startRow, 6].Value = item.SchoolClass;
                worksheet.Cells[startRow, 7].Value = item.CourseLevelStr;
                worksheet.Cells[startRow, 8].Value = ConvertPercent(item.OverallPercent, "-");
                var index = 9;
                if (courseType == EnumCourseType.Academic)
                {
                    foreach (var data in item.OverallModuleReports)
                    {
                        worksheet.Cells[startRow, index].Value = ConvertPercent(data.Percent, "-");
                        index++;
                    }

                    worksheet.Cells[startRow, 22].Value = processDateStr;
                    worksheet.Cells[startRow, 23].Value = item.StatusStr;
                    worksheet.Cells[startRow, 24].Value = expiredDateStr;
                }
                else if (courseType == EnumCourseType.Ielts)
                {
                    foreach (var data in item.OverallModuleReports)
                    {
                        worksheet.Cells[startRow, index].Value = data.Score.HasValue ? data.Score : ConvertPercent(data.Percent, "-");
                        index++;
                        if (data.Type == nameof(EnumMockTestType.FullMockTest))
                        {
                            for (var i = 0; i < CourseProgressValue.SkillFullMocKTest; i++)
                            {
                                double? score = data.SkillScores?.Count > i ? data.SkillScores[i].Scores : null;
                                worksheet.Cells[startRow, index].Value = score.HasValue ? score.Value : ConvertPercent(data.Percent, "-");
                                index++;
                            }
                        }
                    }

                    worksheet.Cells[startRow, 34].Value = processDateStr;
                    worksheet.Cells[startRow, 35].Value = item.StatusStr;
                    worksheet.Cells[startRow, 36].Value = expiredDateStr;
                }
                else
                {
                    foreach (var data in item.OverallModuleReports.Where(x => x.Type == nameof(Domain.Entities.Unit)))
                    {
                        worksheet.Cells[startRow, index].Value = ConvertPercent(data.Percent, "-");
                        index++;
                    }
                    worksheet.Cells[startRow, 21].Value = ConvertPercent(item.OverallModuleReports.FirstOrDefault(x => x.Type == nameof(FinalTest))?.Percent, "-");
                    worksheet.Cells[startRow, 22].Value = processDateStr;
                    worksheet.Cells[startRow, 23].Value = item.StatusStr;
                    worksheet.Cells[startRow, 24].Value = expiredDateStr;
                }
                startRow++;
            }
        }

        private static int GetTotalCount(IList<CourseLevelProgressModel>? courseLevelProgresses, EnumCourseLevel courseLevel)
        {
            return courseLevelProgresses?.FirstOrDefault(x => x.CourseLevel == courseLevel)?.TotalStudent ?? default(int);
        }
    }
}
