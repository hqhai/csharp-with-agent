// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Application.Commands.ReportEventCmd
{
    using System;
    using System.Drawing;
    using System.Globalization;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Helpers;
    using Fsel.Core.Base;
    using Fsel.Core.Caching;
    using Fsel.Course.Domain.Models.EntityModels.ReportEventHaNoi;
    using Fsel.Course.Infrastructure;
    using Fsel.Course.Infrastructure.ValueSettings;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Helpers;
    using MediatR;
    using Microsoft.Data.SqlClient;
    using Microsoft.EntityFrameworkCore;
    using OfficeOpenXml;
    using OfficeOpenXml.Style;

    public class ExportSummaryReportCommand : IRequest<MethodResult<Stream>>
    {
        public int CheckByGroup { get; set; }
    }

    public class ExportSummaryReportCommandHandler : IRequestHandler<ExportSummaryReportCommand, MethodResult<Stream>>
    {
        private readonly CourseDbContext _courseDbContext;
        private readonly ICacheService<IList<ExportSummaryReportCommandModel>> _cacheService;
        private readonly AppSetting _appSetting;
        private readonly AuthContext _authContext;

        public ExportSummaryReportCommandHandler(CourseDbContext courseDbContext,
                                                 ICacheService<IList<ExportSummaryReportCommandModel>> cacheService,
                                                 AppSetting appSetting,
                                                 AuthContext authContext)
        {
            _courseDbContext = courseDbContext;
            _cacheService = cacheService;
            _appSetting = appSetting;
            _authContext = authContext;
        }

        public async Task<MethodResult<Stream>> Handle(ExportSummaryReportCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<Stream> methodResult = new MethodResult<Stream>();

            List<ExportSummaryReportCommandModel> exportSummaryReports;

            var keyCache = $"ExportSummaryReportCommand_{ConvertHelper.Serialize(request)}_{_authContext.CurrentUserId}";
            var data = await _cacheService.GetAsync(keyCache);
            if (data != null && _appSetting.CacheConfig != null && _appSetting.CacheConfig.TurnOnCaching)
            {
                exportSummaryReports = data.ToList();
                methodResult.Result = ExportExcelTemplate(exportSummaryReports, request.CheckByGroup);
                return methodResult;
            }

            var result = await _courseDbContext.Set<ExportSummaryReportCommandModel>()
                                               .FromSqlRaw("EXEC ExportSchoolSummary1  @UserId, @CheckByGroup",
                                                   new SqlParameter("@UserId", _authContext.CurrentUserId),
                                                   new SqlParameter("@CheckByGroup", request.CheckByGroup))
                                               .AsNoTracking()
                                               .ToListAsync(cancellationToken);

            exportSummaryReports = result.ToList();
            if (_appSetting.CacheConfig != null && _appSetting.CacheConfig.TurnOnCaching)
            {
                await _cacheService.SetAsync(keyCache, result, TimeSpan.FromSeconds(_appSetting.CacheConfig.CachingDuration));
            }

            methodResult.Result = ExportExcelTemplate(exportSummaryReports, request.CheckByGroup);
            return methodResult;
        }

        public static Stream ExportExcelTemplate(IList<ExportSummaryReportCommandModel> exportSummaryReports, int checkByGroup)
        {
            ArgumentNullException.ThrowIfNull(exportSummaryReports);
            MemoryStream memoryStream = new MemoryStream();
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using (ExcelPackage excelPackage = new ExcelPackage(new FileInfo(checkByGroup == 1 ? ResourceSettings.DepartmentLevelReport : ResourceSettings.DivisionLevelReport)))
            {
                var excelWorksheet = excelPackage.Workbook.Worksheets[0];
                FillParameterData(excelWorksheet);

                if (checkByGroup == 1)
                {
                    FillDataDepartment(excelWorksheet, exportSummaryReports);
                }
                else
                {
                    FillDataDivision(excelWorksheet, exportSummaryReports);
                }

                excelPackage.SaveAs(memoryStream);
            }

            memoryStream.Position = 0L;
            return memoryStream;
        }

        private static void FillParameterData(ExcelWorksheet excelWorksheet)
        {
            excelWorksheet.Cells["A3"].Value = $"Ngày xuất báo cáo: {DateTime.UtcNow.ConvertTimeFromUtc(EnumCountryKey.Vietnam).ToString("dd/MM/yyyy", CultureInfo.InvariantCulture)}";
        }

        private static void FillDataDepartment(ExcelWorksheet excelWorksheet, IList<ExportSummaryReportCommandModel> exportSummaryReports)
        {
            int startRow = 8;
            foreach (var item in exportSummaryReports)
            {
                BorderRow(excelWorksheet, startRow, "A", "AR");
                excelWorksheet.Cells[startRow, 2].Value = item.DistrictName;
                excelWorksheet.Cells[startRow, 3].Value = item.TotalSchoolTHCSDefault;
                excelWorksheet.Cells[startRow, 4].Value = item.ElevationOfTerrainTHCS;
                excelWorksheet.Cells[startRow, 5].Value = item.ElevationOfRefHeightTHCS;
                excelWorksheet.Cells[startRow, 6].Value = item.TotalStudentDefaultTHCS;
                excelWorksheet.Cells[startRow, 7].Value = item.TotalSchoolTHCS;

                if (item.TotalSchoolTHCSDefault > 0)
                {
                    excelWorksheet.Cells[startRow, 8].Value = $"{Math.Round(((((double)(item.TotalSchoolTHCS ?? 0)) / item.TotalSchoolTHCSDefault) * 100) ?? 0, 2)}%";
                }

                excelWorksheet.Cells[startRow, 9].Value = item.RegisterStudentTHCS;

                if (item.ElevationOfTerrainTHCS > 0)
                {
                    excelWorksheet.Cells[startRow, 10].Value = $"{Math.Round((((double)((item.RegisterStudentTHCS ?? 0)) / item.ElevationOfTerrainTHCS) * 100) ?? 0, 2)}%";
                }

                excelWorksheet.Cells[startRow, 11].Value = item.RegisterTeacherTHCS;

                if (item.ElevationOfRefHeightTHCS > 0)
                {
                    excelWorksheet.Cells[startRow, 12].Value = $"{Math.Round((((double)((item.RegisterTeacherTHCS ?? 0)) / item.ElevationOfRefHeightTHCS) * 100) ?? 0, 2)}%";
                }

                excelWorksheet.Cells[startRow, 13].Value = item.TotalStudentTHCS;
                if (item.TotalStudentDefaultTHCS > 0)
                {
                    excelWorksheet.Cells[startRow, 14].Value = $"{Math.Round(((((double)(item.TotalStudentTHCS ?? 0)) / item.TotalStudentDefaultTHCS) * 100) ?? 0, 2)}%";
                }

                excelWorksheet.Cells[startRow, 15].Value = item.TotalStudentVerifiedTHCS;
                if (item.TotalStudentVerifiedTHCS > 0)
                {
                    excelWorksheet.Cells[startRow, 16].Value = $"{Math.Round(((((double)(item.TotalStudentTHCS ?? 0)) / item.ElevationOfTerrainTHCS) * 100) ?? 0, 2)}%";
                }

                startRow++; // Di chuyển xuống dòng tiếp theo
            }

            foreach (var item in exportSummaryReports)
            {
                if (exportSummaryReports.IndexOf(item) == 0)
                {
                    excelWorksheet.Cells[$"A{startRow}"].Value = "THPT & Liên cấp";
                    excelWorksheet.Cells[$"A{startRow}"].Style.Font.Bold = true;
                    startRow++;
                }

                BorderRow(excelWorksheet, startRow, "A", "N");

                excelWorksheet.Cells[startRow, 2].Value = item.DistrictName;
                excelWorksheet.Cells[startRow, 3].Value = item.TotalSchoolDefault - item.TotalSchoolTHCSDefault;
                excelWorksheet.Cells[startRow, 4].Value = item.ElevationOfTerrain - item.ElevationOfTerrainTHCS;
                excelWorksheet.Cells[startRow, 5].Value = item.ElevationOfRefHeight - item.ElevationOfRefHeightTHCS;
                excelWorksheet.Cells[startRow, 6].Value = item.TotalStudentDefault - item.TotalStudentDefaultTHCS;
                excelWorksheet.Cells[startRow, 7].Value = item.TotalSchool - item.TotalSchoolTHCS;

                if (item.TotalSchoolDefault > 0)
                {
                    excelWorksheet.Cells[startRow, 8].Value = $"{Math.Round(((((double)((item.TotalSchool - item.TotalSchoolTHCS) ?? 0)) / (item.TotalSchoolDefault - item.TotalSchoolTHCSDefault)) * 100) ?? 0, 2)}%";
                }

                excelWorksheet.Cells[startRow, 9].Value = item.RegisterStudent - item.RegisterStudentTHCS;

                if (item.ElevationOfTerrain > 0)
                {
                    excelWorksheet.Cells[startRow, 10].Value = $"{Math.Round(((((double)((item.RegisterStudent - item.RegisterStudentTHCS) ?? 0)) / (item.ElevationOfTerrain - item.ElevationOfTerrainTHCS)) * 100) ?? 0, 2)}%";
                }

                excelWorksheet.Cells[startRow, 11].Value = item.RegisterTeacher - item.RegisterTeacherTHCS;

                if (item.ElevationOfRefHeight > 0)
                {
                    excelWorksheet.Cells[startRow, 12].Value = $"{Math.Round(((((double)((item.RegisterTeacher - item.RegisterTeacherTHCS) ?? 0)) / (item.ElevationOfRefHeight - item.ElevationOfRefHeightTHCS)) * 100) ?? 0, 2)}%";
                }

                excelWorksheet.Cells[startRow, 13].Value = item.TotalStudent - item.TotalStudentTHCS;

                if (item.TotalStudentDefault > 0)
                {
                    excelWorksheet.Cells[startRow, 14].Value = $"{Math.Round(((((double)((item.TotalStudent - item.TotalStudentTHCS) ?? 0)) / (item.TotalStudentDefault - item.TotalStudentDefaultTHCS)) * 100) ?? 0, 2)}%";
                }

                startRow++; // Di chuyển xuống dòng tiếp theo

                if (exportSummaryReports.IndexOf(item) == (exportSummaryReports.Count - 1))
                {
                    excelWorksheet.Cells[$"B{startRow}"].Value = "Tổng";
                    excelWorksheet.Cells[$"B{startRow}"].Style.Font.Bold = true;
                    excelWorksheet.Cells[$"A{startRow}:N{startRow}"].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    excelWorksheet.Cells[$"A{startRow}:N{startRow}"].Style.Fill.BackgroundColor.SetColor(Color.Yellow);
                    excelWorksheet.Cells[$"C{startRow}"].Value = exportSummaryReports.Sum(x => x.TotalSchoolDefault);
                    excelWorksheet.Cells[$"D{startRow}"].Value = exportSummaryReports.Sum(x => x.ElevationOfTerrain);
                    excelWorksheet.Cells[$"E{startRow}"].Value = exportSummaryReports.Sum(x => x.ElevationOfRefHeight);
                    excelWorksheet.Cells[$"F{startRow}"].Value = exportSummaryReports.Sum(x => x.TotalStudentDefault);
                    excelWorksheet.Cells[$"G{startRow}"].Value = exportSummaryReports.Sum(x => x.TotalSchool);

                    if (item.TotalSchoolDefault > 0)
                    {
                        var totalSchoolTotal = exportSummaryReports.Sum(x => (double)(x.TotalSchool ?? 0));
                        var totalSchoolDefaultTotal = exportSummaryReports.Sum(x => (double)(x.TotalSchoolDefault ?? 0));
                        excelWorksheet.Cells[$"H{startRow}"].Value = $"{Math.Round(((totalSchoolTotal / totalSchoolDefaultTotal) * 100), 2)}%";
                    }

                    excelWorksheet.Cells[$"I{startRow}"].Value = exportSummaryReports.Sum(x => x.RegisterStudent);

                    if (item.ElevationOfTerrain > 0)
                    {
                        var registerStudentTotal = exportSummaryReports.Sum(x => (double)(x.RegisterStudent ?? 0));
                        var elevationOfTerrainTotal = exportSummaryReports.Sum(x => (double)(x.ElevationOfTerrain ?? 0));
                        excelWorksheet.Cells[$"J{startRow}"].Value = $"{Math.Round(((registerStudentTotal / elevationOfTerrainTotal) * 100), 2)}%";
                    }

                    excelWorksheet.Cells[$"K{startRow}"].Value = exportSummaryReports.Sum(x => x.RegisterTeacher);

                    if (item.ElevationOfRefHeight > 0)
                    {
                        var registerTeacherTotal = exportSummaryReports.Sum(x => (double)(x.RegisterTeacher ?? 0));
                        var elevationOfRefHeight = exportSummaryReports.Sum(x => (double)(x.ElevationOfRefHeight ?? 0));
                        excelWorksheet.Cells[$"L{startRow}"].Value = $"{Math.Round(((registerTeacherTotal / elevationOfRefHeight) * 100), 2)}%";
                    }

                    excelWorksheet.Cells[$"M{startRow}"].Value = exportSummaryReports.Sum(x => x.TotalStudent);

                    if (item.TotalStudentDefault > 0)
                    {
                        var totalStudentTotal = exportSummaryReports.Sum(x => (double)(x.TotalStudent ?? 0));
                        var totalStudentDefaultTotal = exportSummaryReports.Sum(x => (double)(x.TotalStudentDefault ?? 0));
                        excelWorksheet.Cells[$"N{startRow}"].Value = $"{Math.Round(((totalStudentTotal / totalStudentDefaultTotal) * 100), 2)}%";
                    }

                    BorderRow(excelWorksheet, startRow, "A", "N");
                }
            }
        }

        private static void FillDataDivision(ExcelWorksheet excelWorksheet, IList<ExportSummaryReportCommandModel> exportSummaryReports)
        {
            int startRow = 8;
            int countData = 0;
            int countLevelTHCS = 0;
            int levelTHCS = exportSummaryReports.Count(x => x.Level == 3);
            foreach (var item in exportSummaryReports.OrderBy(x => x.Level))
            {
                countData++;
                countLevelTHCS++;

                BorderRow(excelWorksheet, startRow, "A", "K");

                excelWorksheet.Cells[startRow, 2].Value = item.SchoolName;
                excelWorksheet.Cells[startRow, 3].Value = item.ElevationOfTerrain;
                excelWorksheet.Cells[startRow, 4].Value = item.ElevationOfRefHeight;
                excelWorksheet.Cells[startRow, 5].Value = item.TotalStudentDefault;
                excelWorksheet.Cells[startRow, 6].Value = item.RegisterStudent;
                if (item.ElevationOfTerrain > 0)
                {
                    excelWorksheet.Cells[startRow, 7].Value = $"{Math.Round(((((double)(item.RegisterStudent ?? 0)) / item.ElevationOfTerrain) * 100) ?? 0, 2)}%";
                }
                excelWorksheet.Cells[startRow, 8].Value = item.RegisterTeacher;
                if (item.ElevationOfRefHeight > 0)
                {
                    excelWorksheet.Cells[startRow, 9].Value = $"{Math.Round(((((double)(item.RegisterTeacher ?? 0)) / item.ElevationOfRefHeight) * 100) ?? 0, 2)}%";
                }
                excelWorksheet.Cells[startRow, 10].Value = item.TotalStudent;
                if (item.TotalStudentDefault > 0)
                {
                    excelWorksheet.Cells[startRow, 11].Value = $"{Math.Round(((((double)(item.TotalStudent ?? 0)) / item.TotalStudentDefault) * 100) ?? 0, 2)}%";
                }

                // New
                excelWorksheet.Cells[startRow, 12].Value = item.TotalStudentVerified;
                if (item.TotalStudentVerified > 0)
                {
                    excelWorksheet.Cells[startRow, 13].Value = $"{Math.Round(((((double)(item.TotalStudentVerified ?? 0)) / item.ElevationOfTerrain) * 100) ?? 0, 2)}%";
                }

                excelWorksheet.Cells[startRow, 14].Value = item.TotalTeacherVerified;
                if (item.TotalTeacherVerified > 0)
                {
                    excelWorksheet.Cells[startRow, 15].Value = $"{Math.Round(((((double)(item.TotalTeacherVerified ?? 0)) / item.ElevationOfRefHeight) * 100) ?? 0, 2)}%";
                }

                var totalVerified = (item.TotalTeacherVerified ?? default) + (item.TotalStudentVerified ?? default);
                excelWorksheet.Cells[startRow, 16].Value = totalVerified;
                if (totalVerified > 0)
                {
                    excelWorksheet.Cells[startRow, 17].Value = $"{Math.Round(((((double)totalVerified) / item.TotalStudentDefault) * 100) ?? 0, 2)}%";
                }

                excelWorksheet.Cells[startRow, 18].Value = item.TotalStudentPTComplete;
                if (item.TotalStudentPTComplete > 0)
                {
                    excelWorksheet.Cells[startRow, 19].Value = $"{Math.Round(((((double)(item.TotalStudentPTComplete ?? 0)) / item.RegisterStudent) * 100) ?? 0, 2)}%";
                    excelWorksheet.Cells[startRow, 20].Value = $"{Math.Round(((((double)(item.TotalStudentPTComplete ?? 0)) / item.ElevationOfTerrain) * 100) ?? 0, 2)}%";
                }

                excelWorksheet.Cells[startRow, 21].Value = item.TotalStudentPTProgress;
                if (item.TotalStudentPTProgress > 0)
                {
                    excelWorksheet.Cells[startRow, 22].Value = $"{Math.Round(((((double)(item.TotalStudentPTProgress ?? 0)) / item.RegisterStudent) * 100) ?? 0, 2)}%";
                    excelWorksheet.Cells[startRow, 23].Value = $"{Math.Round(((((double)(item.TotalStudentPTProgress ?? 0)) / item.ElevationOfTerrain) * 100) ?? 0, 2)}%";
                }

                excelWorksheet.Cells[startRow, 24].Value = item.TotalTeacherPTComplete;
                if (item.TotalTeacherPTComplete > 0)
                {
                    excelWorksheet.Cells[startRow, 25].Value = $"{Math.Round(((((double)(item.TotalTeacherPTComplete ?? 0)) / item.RegisterTeacher) * 100) ?? 0, 2)}%";
                    excelWorksheet.Cells[startRow, 26].Value = $"{Math.Round(((((double)(item.TotalTeacherPTComplete ?? 0)) / item.ElevationOfRefHeight) * 100) ?? 0, 2)}%";
                }

                excelWorksheet.Cells[startRow, 27].Value = item.TotalTeacherPTProgress;
                if (item.TotalTeacherPTProgress > 0)
                {
                    excelWorksheet.Cells[startRow, 28].Value = $"{Math.Round(((((double)(item.TotalTeacherPTProgress ?? 0)) / item.RegisterTeacher) * 100) ?? 0, 2)}%";
                    excelWorksheet.Cells[startRow, 29].Value = $"{Math.Round(((((double)(item.TotalTeacherPTProgress ?? 0)) / item.ElevationOfRefHeight) * 100) ?? 0, 2)}%";
                }

                var totalPTComplete = (item.TotalTeacherPTComplete ?? default) + (item.TotalStudentPTComplete ?? default);
                var totalPTProgress = (item.TotalTeacherPTProgress ?? default) + (item.TotalStudentPTProgress ?? default);
                var totalRegister = (item.RegisterTeacher ?? default) + (item.RegisterStudent ?? default);
                excelWorksheet.Cells[startRow, 30].Value = totalPTComplete;
                if (totalPTComplete > 0)
                {
                    excelWorksheet.Cells[startRow, 31].Value = $"{Math.Round(((((double)totalPTComplete) / item.TotalStudentDefault) * 100) ?? 0, 2)}%";
                    excelWorksheet.Cells[startRow, 32].Value = $"{Math.Round(((((double)totalPTComplete) / totalRegister) * 100), 2)}%";
                }

                excelWorksheet.Cells[startRow, 33].Value = totalPTProgress;
                if (totalPTComplete > 0)
                {
                    excelWorksheet.Cells[startRow, 34].Value = $"{Math.Round(((((double)totalPTProgress) / item.TotalStudentDefault) * 100) ?? 0, 2)}%";
                    excelWorksheet.Cells[startRow, 35].Value = $"{Math.Round(((((double)totalPTProgress) / totalRegister) * 100), 2)}%";
                }

                excelWorksheet.Cells[startRow, 36].Value = item.TotalStudentLearnProgress;
                if (item.TotalStudentLearnProgress > 0)
                {
                    excelWorksheet.Cells[startRow, 37].Value = $"{Math.Round(((((double)(item.TotalStudentLearnProgress ?? 0)) / item.RegisterStudent) * 100) ?? 0, 2)}%";
                }

                excelWorksheet.Cells[startRow, 38].Value = item.TotalTeacherLearnProgress;
                if (item.TotalTeacherLearnProgress > 0)
                {
                    excelWorksheet.Cells[startRow, 39].Value = $"{Math.Round(((((double)(item.TotalTeacherLearnProgress ?? 0)) / item.RegisterTeacher) * 100) ?? 0, 2)}%";
                }
                var totalLearn = (item.TotalStudentLearnProgress ?? default) + (item.TotalTeacherLearnProgress ?? default);
                excelWorksheet.Cells[startRow, 40].Value = totalLearn;
                if (totalLearn > 0)
                {
                    excelWorksheet.Cells[startRow, 41].Value = $"{Math.Round(((((double)totalLearn) / totalRegister) * 100), 2)}%";
                }

                startRow++; // Di chuyển xuống dòng tiếp theo

                if (countLevelTHCS == levelTHCS)
                {
                    excelWorksheet.Cells[$"A{startRow}"].Value = "THPT & Liên cấp";
                    excelWorksheet.Cells[$"A{startRow}"].Style.Font.Bold = true;
                    startRow++;
                }

                if (countData == exportSummaryReports.Count)
                {
                    var registerStudentTotal = exportSummaryReports.Sum(x => (double)(x.RegisterStudent ?? 0));
                    var registerTeacherTotal = exportSummaryReports.Sum(x => (double)(x.RegisterTeacher ?? 0));
                    var registerAccountTotal = registerStudentTotal + registerTeacherTotal;

                    var elevationOfTerrainTotal = exportSummaryReports.Sum(x => (double)(x.ElevationOfTerrain ?? 0));
                    var elevationOfRefHeight = exportSummaryReports.Sum(x => (double)(x.ElevationOfRefHeight ?? 0));
                    var totalStudentDefault = exportSummaryReports.Sum(x => (double)(x.TotalStudentDefault ?? 0));

                    var totalStudentTotal = exportSummaryReports.Sum(x => (double)(x.TotalStudent ?? 0));
                    // Verified
                    var totalStudentVerified = exportSummaryReports.Sum(x => (double)(x.TotalStudentVerified ?? 0));
                    var totalTeachertVerified = exportSummaryReports.Sum(x => (double)(x.TotalTeacherVerified ?? 0));
                    var totalAccountVerified = totalStudentVerified + totalTeachertVerified;
                    // PT Complete
                    var totalStudentPTComplete = exportSummaryReports.Sum(x => (double)(x.TotalStudentPTComplete ?? 0));
                    var totalTeacherPTComplete = exportSummaryReports.Sum(x => (double)(x.TotalTeacherPTComplete ?? 0));
                    var totalAccountPTComplete = totalTeacherPTComplete + totalStudentPTComplete;
                    // PT Progress
                    var totalStudentPTProgress = exportSummaryReports.Sum(x => (double)(x.TotalStudentPTProgress ?? 0));
                    var totalTeacherPTProgress = exportSummaryReports.Sum(x => (double)(x.TotalTeacherPTProgress ?? 0));
                    var totalAccountPTProgress = totalTeacherPTProgress + totalStudentPTProgress;

                    // Learn
                    var totalStudentLearn = exportSummaryReports.Sum(x => (double)(x.TotalStudentLearnProgress ?? 0));
                    var totalTeacherLearn = exportSummaryReports.Sum(x => (double)(x.TotalTeacherLearnProgress ?? 0));
                    var totalAccountLearn = totalStudentLearn + totalTeacherLearn;

                    excelWorksheet.Cells[$"A{startRow}:AO{startRow}"].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    excelWorksheet.Cells[$"A{startRow}:AO{startRow}"].Style.Fill.BackgroundColor.SetColor(Color.Yellow);
                    excelWorksheet.Cells[$"B{startRow}"].Value = "Tổng";
                    excelWorksheet.Cells[$"B{startRow}"].Style.Font.Bold = true;
                    excelWorksheet.Cells[$"C{startRow}"].Value = elevationOfTerrainTotal;
                    excelWorksheet.Cells[$"D{startRow}"].Value = elevationOfRefHeight;
                    excelWorksheet.Cells[$"E{startRow}"].Value = totalStudentDefault;
                    excelWorksheet.Cells[$"F{startRow}"].Value = registerStudentTotal;
                    excelWorksheet.Cells[$"G{startRow}"].Value = $"{registerStudentTotal.GetPercent(elevationOfTerrainTotal, 2)}%";

                    excelWorksheet.Cells[$"H{startRow}"].Value = registerTeacherTotal;
                    excelWorksheet.Cells[$"I{startRow}"].Value = $"{registerTeacherTotal.GetPercent(elevationOfRefHeight, 2)}%";

                    excelWorksheet.Cells[$"J{startRow}"].Value = totalStudentTotal;
                    excelWorksheet.Cells[$"K{startRow}"].Value = $"{totalStudentTotal.GetPercent(totalStudentDefault, 2)}%";

                    excelWorksheet.Cells[$"L{startRow}"].Value = totalStudentVerified;
                    excelWorksheet.Cells[$"M{startRow}"].Value = $"{totalStudentVerified.GetPercent(elevationOfTerrainTotal, 2)}%";

                    excelWorksheet.Cells[$"N{startRow}"].Value = totalTeachertVerified;
                    excelWorksheet.Cells[$"O{startRow}"].Value = $"{totalTeachertVerified.GetPercent(elevationOfRefHeight, 2)}%";

                    excelWorksheet.Cells[$"P{startRow}"].Value = totalAccountVerified;
                    excelWorksheet.Cells[$"Q{startRow}"].Value = $"{totalAccountVerified.GetPercent(totalStudentDefault, 2)}%";

                    excelWorksheet.Cells[$"R{startRow}"].Value = totalStudentPTComplete;
                    excelWorksheet.Cells[$"S{startRow}"].Value = $"{totalStudentPTComplete.GetPercent(registerStudentTotal, 2)}%";
                    excelWorksheet.Cells[$"T{startRow}"].Value = $"{totalStudentPTComplete.GetPercent(elevationOfTerrainTotal, 2)}%";

                    excelWorksheet.Cells[$"U{startRow}"].Value = totalStudentPTProgress;
                    excelWorksheet.Cells[$"V{startRow}"].Value = $"{totalStudentPTProgress.GetPercent(registerStudentTotal, 2)}%";
                    excelWorksheet.Cells[$"W{startRow}"].Value = $"{totalStudentPTProgress.GetPercent(elevationOfTerrainTotal, 2)}%";

                    excelWorksheet.Cells[$"X{startRow}"].Value = totalTeacherPTComplete;
                    excelWorksheet.Cells[$"Y{startRow}"].Value = $"{totalTeacherPTComplete.GetPercent(registerTeacherTotal, 2)}%";
                    excelWorksheet.Cells[$"Z{startRow}"].Value = $"{totalTeacherPTComplete.GetPercent(elevationOfRefHeight, 2)}%";

                    excelWorksheet.Cells[$"AA{startRow}"].Value = totalTeacherPTProgress;
                    excelWorksheet.Cells[$"AB{startRow}"].Value = $"{totalTeacherPTProgress.GetPercent(registerTeacherTotal, 2)}%";
                    excelWorksheet.Cells[$"AC{startRow}"].Value = $"{totalTeacherPTProgress.GetPercent(elevationOfRefHeight, 2)}%";

                    excelWorksheet.Cells[$"AD{startRow}"].Value = totalAccountPTComplete;
                    excelWorksheet.Cells[$"AE{startRow}"].Value = $"{totalAccountPTComplete.GetPercent(registerAccountTotal, 2)}%";
                    excelWorksheet.Cells[$"AF{startRow}"].Value = $"{totalAccountPTComplete.GetPercent(totalStudentDefault, 2)}%";

                    excelWorksheet.Cells[$"AG{startRow}"].Value = totalAccountPTProgress;
                    excelWorksheet.Cells[$"AH{startRow}"].Value = $"{totalAccountPTProgress.GetPercent(registerAccountTotal, 2)}%";
                    excelWorksheet.Cells[$"AI{startRow}"].Value = $"{totalAccountPTProgress.GetPercent(totalStudentDefault, 2)}%";

                    excelWorksheet.Cells[$"AJ{startRow}"].Value = totalStudentLearn;
                    excelWorksheet.Cells[$"AK{startRow}"].Value = $"{totalStudentLearn.GetPercent(elevationOfTerrainTotal, 2)}%";
                    excelWorksheet.Cells[$"AL{startRow}"].Value = totalTeacherLearn;
                    excelWorksheet.Cells[$"AM{startRow}"].Value = $"{totalTeacherLearn.GetPercent(elevationOfRefHeight, 2)}%";
                    excelWorksheet.Cells[$"AN{startRow}"].Value = totalAccountLearn;
                    excelWorksheet.Cells[$"AO{startRow}"].Value = $"{totalAccountLearn.GetPercent(totalStudentDefault, 2)}%";
                    BorderRow(excelWorksheet, startRow, "A", "AO");
                }
            }
        }

        public static string GetData(object data, object? param)
        {
            string objStr = data?.ToString() ?? string.Empty;
            return string.Format(objStr, param);
        }

        private static void BorderRow(ExcelWorksheet excelWorksheet, int startRow, string startColumn, string endColumn)
        {
            // border viền
            var range = excelWorksheet.Cells[$"{startColumn}{startRow}:{endColumn}{startRow}"];
            range.Style.Border.Top.Style = ExcelBorderStyle.Thin;
            range.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
            range.Style.Border.Left.Style = ExcelBorderStyle.Thin;
            range.Style.Border.Right.Style = ExcelBorderStyle.Thin;
        }
    }
}
