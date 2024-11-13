// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Commands.ManagerReportCmd
{
    using System.Globalization;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Helpers;
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

        public static Stream ExportExcelTemplate(ExportFileExcelReportLearningResultCommand request, IList<LearningResultReportModel>? learningResultReports, OverallReportLearningResultModel? overallReport, string? schoolName)
        {
            ArgumentNullException.ThrowIfNull(request);
            MemoryStream memoryStream = new MemoryStream();
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using (ExcelPackage excelPackage = new ExcelPackage(new FileInfo(request.CourseType == EnumCourseType.Academic ? ResourceSettings.ManagerReportLearningResultAcaExcel : ResourceSettings.ManagerReportLearningResultIELTSExcel)))
            {
                var excelWorksheet = excelPackage.Workbook.Worksheets[0];
                if (request.CourseType == EnumCourseType.Ielts)
                {
                    excelWorksheet.Cells["E2"].Value = schoolName;
                    excelWorksheet.Cells["E3"].Value = overallReport?.TotalStudent;
                    excelWorksheet.Cells["E7"].Value = ConvertPercent(overallReport?.OverallAvgPercent);

                    excelWorksheet.Cells["F2"].Value = GetData(excelWorksheet.Cells["F2"].Value, request.LearningStatus?.GetDescription());
                    excelWorksheet.Cells["G2"].Value = GetData(excelWorksheet.Cells["G2"].Value, request.SchoolGrade);
                    excelWorksheet.Cells["H2"].Value = GetData(excelWorksheet.Cells["H2"].Value, request.SchoolClass);
                    excelWorksheet.Cells["I2"].Value = GetData(excelWorksheet.Cells["I2"].Value, request.CourseLevel);
                    excelWorksheet.Cells["J2"].Value = GetData(excelWorksheet.Cells["J2"].Value, request.OverallScore?.GetDescription());
                    excelWorksheet.Cells["K2"].Value = GetData(excelWorksheet.Cells["K2"].Value, request.EndDate.HasValue ? request.EndDate.Value.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture) : string.Empty);
                    excelWorksheet.Cells["L2"].Value = GetData(excelWorksheet.Cells["L2"].Value, DateTime.UtcNow.ConvertTimeFromUtc(EnumCountryKey.Vietnam).ToString("dd/MM/yyyy  hh:mm tt", CultureInfo.InvariantCulture));
                }
                else
                {
                    excelWorksheet.Cells["F2"].Value = schoolName;
                    excelWorksheet.Cells["F3"].Value = overallReport?.TotalStudent;
                    excelWorksheet.Cells["F5"].Value = ConvertPercent(overallReport?.OverallAvgPercent);
                    excelWorksheet.Cells["F8"].Value = ConvertPercent(overallReport?.OverallAvgPercentFinal);

                    excelWorksheet.Cells["G2"].Value = GetData(excelWorksheet.Cells["G2"].Value, request.LearningStatus?.GetDescription());
                    excelWorksheet.Cells["H2"].Value = GetData(excelWorksheet.Cells["H2"].Value, request.SchoolGrade);
                    excelWorksheet.Cells["I2"].Value = GetData(excelWorksheet.Cells["I2"].Value, request.SchoolClass);
                    excelWorksheet.Cells["J2"].Value = GetData(excelWorksheet.Cells["J2"].Value, request.CourseLevel);
                    excelWorksheet.Cells["K2"].Value = GetData(excelWorksheet.Cells["K2"].Value, request.OverallScore?.GetDescription());
                    excelWorksheet.Cells["L2"].Value = GetData(excelWorksheet.Cells["L2"].Value, request.EndDate.HasValue ? request.EndDate.Value.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture) : string.Empty);
                    excelWorksheet.Cells["M2"].Value = GetData(excelWorksheet.Cells["M2"].Value, DateTime.UtcNow.ConvertTimeFromUtc(EnumCountryKey.Vietnam).ToString("dd/MM/yyyy hh:mm tt", CultureInfo.InvariantCulture));
                }
                FillUnitData(excelWorksheet, request.CourseType, overallReport);
                FillCourseLevelData(excelWorksheet, request.CourseType, overallReport);
                if (learningResultReports != null && learningResultReports.Any())
                {
                    FillLearningProgressData(excelWorksheet, request.CourseType, learningResultReports);
                }
                excelPackage.SaveAs(memoryStream);
            }

            memoryStream.Position = 0L;
            return memoryStream;
        }

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
                var startRow = 6;
                foreach (var item in courseLevelProgress)
                {
                    excelWorksheet.Cells[6, startRow].Value = GetData(excelWorksheet.Cells[6, startRow].Value, ConvertPercent(item.Percent));
                    excelWorksheet.Cells[7, startRow].Value = item.TotalStudent + " hs";
                    startRow++;
                }
            }
            else
            {
                var startRow = 5;
                foreach (var item in courseLevelProgress)
                {
                    excelWorksheet.Cells[8, startRow].Value = GetData(excelWorksheet.Cells[8, startRow].Value, ConvertPercent(item.Percent));
                    excelWorksheet.Cells[9, startRow].Value = item.TotalStudent + " hs";
                    startRow++;
                }
            }
        }

        private static void FillCourseLevelData(ExcelWorksheet excelWorksheet, EnumCourseType courseType, OverallReportLearningResultModel? overallReport)
        {
            var courseLevelProgress = overallReport?.CourseLevelProgresses;
            if (courseType == EnumCourseType.Academic)
            {
                excelWorksheet.Cells["F4"].Value = GetData(excelWorksheet.Cells["F4"].Value, GetTotalCount(courseLevelProgress, EnumCourseLevel.A1));
                excelWorksheet.Cells["G4"].Value = GetData(excelWorksheet.Cells["G4"].Value, GetTotalCount(courseLevelProgress, EnumCourseLevel.A2));
                excelWorksheet.Cells["H4"].Value = GetData(excelWorksheet.Cells["H4"].Value, GetTotalCount(courseLevelProgress, EnumCourseLevel.B1));
                excelWorksheet.Cells["I4"].Value = GetData(excelWorksheet.Cells["I4"].Value, GetTotalCount(courseLevelProgress, EnumCourseLevel.B1Plus));
                excelWorksheet.Cells["J4"].Value = GetData(excelWorksheet.Cells["J4"].Value, GetTotalCount(courseLevelProgress, EnumCourseLevel.B2));
                excelWorksheet.Cells["K4"].Value = GetData(excelWorksheet.Cells["K4"].Value, GetTotalCount(courseLevelProgress, EnumCourseLevel.C1));
            }
            else
            {
                var ms1Progress = courseLevelProgress?.FirstOrDefault(x => x.CourseLevel == EnumCourseLevel.MS1);
                excelWorksheet.Cells["E4"].Value = GetData(excelWorksheet.Cells["E4"].Value, ms1Progress?.TotalStudent ?? default);
                var ms1ProgressFirst = ms1Progress?.OverallTestResults?.FirstOrDefault();
                var ms1ProgressLast = ms1Progress?.OverallTestResults?.LastOrDefault();
                excelWorksheet.Cells["E5"].Value = GetData(excelWorksheet.Cells["E5"].Value, new string[] { $"{ms1ProgressFirst?.Score ?? default}", $"{ms1ProgressFirst?.TotalStudent ?? default}" });
                excelWorksheet.Cells["E6"].Value = GetData(excelWorksheet.Cells["E6"].Value, new string[] { $"{ms1ProgressLast?.Score ?? default}", $"{ms1ProgressLast?.TotalStudent ?? default}" });

                var ms2Progress = courseLevelProgress?.FirstOrDefault(x => x.CourseLevel == EnumCourseLevel.MS2);
                excelWorksheet.Cells["F4"].Value = GetData(excelWorksheet.Cells["F4"].Value, ms2Progress?.TotalStudent ?? default);
                var ms2ProgressFirst = ms2Progress?.OverallTestResults?.FirstOrDefault();
                var ms2ProgressLast = ms2Progress?.OverallTestResults?.LastOrDefault();
                excelWorksheet.Cells["F5"].Value = GetData(excelWorksheet.Cells["F5"].Value, new string[] { $"{ms2ProgressFirst?.Score ?? default}", $"{ms2ProgressFirst?.TotalStudent ?? default}" });
                excelWorksheet.Cells["F6"].Value = GetData(excelWorksheet.Cells["F6"].Value, new string[] { $"{ms2ProgressLast?.Score ?? default}", $"{ms2ProgressLast?.TotalStudent ?? default}" });

                var ms3Progress = courseLevelProgress?.FirstOrDefault(x => x.CourseLevel == EnumCourseLevel.MS3);
                excelWorksheet.Cells["G4"].Value = GetData(excelWorksheet.Cells["G4"].Value, ms3Progress?.TotalStudent ?? default);
                var ms3ProgressFirst = ms3Progress?.OverallTestResults?.FirstOrDefault();
                var ms3ProgressLast = ms3Progress?.OverallTestResults?.LastOrDefault();
                excelWorksheet.Cells["G5"].Value = GetData(excelWorksheet.Cells["G5"].Value, new string[] { $"{ms3ProgressFirst?.Score ?? default}", $"{ms3ProgressFirst?.TotalStudent ?? default}" });
                excelWorksheet.Cells["G6"].Value = GetData(excelWorksheet.Cells["G6"].Value, new string[] { $"{ms3ProgressLast?.Score ?? default}", $"{ms3ProgressLast?.TotalStudent ?? default}" });
            }
        }

        private static void FillLearningProgressData(ExcelWorksheet worksheet, EnumCourseType courseType, IList<LearningResultReportModel> learningProgressReports)
        {
            int startRow = courseType == EnumCourseType.Ielts ? 11 : 10;

            foreach (var item in learningProgressReports)
            {
                worksheet.Cells[startRow, 1].Value = item.FullName;
                worksheet.Cells[startRow, 2].Value = item.Email;
                worksheet.Cells[startRow, 3].Value = item.SchoolName;
                worksheet.Cells[startRow, 4].Value = item.SchoolGrade;
                worksheet.Cells[startRow, 5].Value = item.SchoolClass;
                worksheet.Cells[startRow, 6].Value = item.CourseLevelStr;
                worksheet.Cells[startRow, 7].Value = ConvertPercent(item.OverallPercent, "-");
                var index = 8;
                if (courseType == EnumCourseType.Academic)
                {
                    foreach (var data in item.OverallModuleReports)
                    {
                        worksheet.Cells[startRow, index].Value = ConvertPercent(data.Percent, "-");
                        index++;
                    }

                    worksheet.Cells[startRow, 21].Value = item.ProcessDate.HasValue ? item.ProcessDate.Value.ConvertTimeFromUtc(EnumCountryKey.Vietnam).ToString("dd/MM/yyyy hh:mm tt", CultureInfo.InvariantCulture) : string.Empty;
                    worksheet.Cells[startRow, 22].Value = item.StatusStr;
                    worksheet.Cells[startRow, 23].Value = item.ExpiredDate.HasValue ? item.ExpiredDate.Value.ConvertTimeFromUtc(EnumCountryKey.Vietnam).ToString("dd/MM/yyyy hh:mm tt", CultureInfo.InvariantCulture) : string.Empty;
                }
                else
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

                    worksheet.Cells[startRow, 34].Value = item.ProcessDate.HasValue ? item.ProcessDate.Value.ConvertTimeFromUtc(EnumCountryKey.Vietnam).ToString("dd/MM/yyyy hh:mm tt", CultureInfo.InvariantCulture) : string.Empty;
                    worksheet.Cells[startRow, 35].Value = item.StatusStr;
                    worksheet.Cells[startRow, 36].Value = item.ExpiredDate.HasValue ? item.ExpiredDate.Value.ConvertTimeFromUtc(EnumCountryKey.Vietnam).ToString("dd/MM/yyyy hh:mm tt", CultureInfo.InvariantCulture) : string.Empty;
                }
                startRow++;
            }
        }

        private static string GetData(object data, object? param)
        {
            string objStr = data?.ToString() ?? string.Empty;
            return string.Format(objStr, param);
        }

        private static string GetData(object data, params object[] param)
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
