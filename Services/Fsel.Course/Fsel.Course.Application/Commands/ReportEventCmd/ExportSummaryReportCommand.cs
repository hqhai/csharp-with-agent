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
    using Fsel.Common.Caching;
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
            foreach (var item in exportSummaryReports.OrderByDescending(x => x.TotalStudentDefault))
            {
                BorderRow(excelWorksheet, startRow, "A", "AR");
                excelWorksheet.Cells[startRow, 2].Value = item.DistrictName;
                excelWorksheet.Cells[startRow, 3].Value = item.TotalSchoolTHCSDefault;
                excelWorksheet.Cells[startRow, 4].Value = item.ElevationOfTerrainTHCS;
                excelWorksheet.Cells[startRow, 5].Value = item.ElevationOfRefHeightTHCS;
                excelWorksheet.Cells[startRow, 6].Value = item.TotalStudentDefaultTHCS;
                excelWorksheet.Cells[startRow, 7].Value = item.TotalSchoolTHCS;
                excelWorksheet.Cells[startRow, 8].Value = $"{(double)(item.TotalSchoolTHCS ?? 0).GetPercent((item.TotalSchoolTHCSDefault ?? default), 2)}%";

                excelWorksheet.Cells[startRow, 9].Value = item.RegisterStudentTHCS;
                excelWorksheet.Cells[startRow, 10].Value = $"{(double)((item.RegisterStudentTHCS ?? 0)).GetPercent((item.ElevationOfTerrainTHCS ?? default), 2)}%";

                excelWorksheet.Cells[startRow, 11].Value = item.RegisterTeacherTHCS;
                excelWorksheet.Cells[startRow, 12].Value = $"{(double)((item.RegisterTeacherTHCS ?? 0)).GetPercent((item.ElevationOfRefHeightTHCS ?? default), 2)}%";

                excelWorksheet.Cells[startRow, 13].Value = item.TotalStudentTHCS;
                excelWorksheet.Cells[startRow, 14].Value = $"{(double)(item.TotalStudentTHCS ?? 0).GetPercent((item.TotalStudentDefaultTHCS ?? default), 2)}%";

                //Số HS xác thực thành công
                excelWorksheet.Cells[startRow, 15].Value = item.TotalStudentVerifiedTHCS;
                excelWorksheet.Cells[startRow, 16].Value = $"{(double)(item.TotalStudentVerifiedTHCS ?? default).GetPercent((item.ElevationOfTerrainTHCS ?? default), 2)}%";

                //Số lượng GV xác thực thành công
                excelWorksheet.Cells[startRow, 17].Value = item.TotalTeacherVerifiedTHCS;
                excelWorksheet.Cells[startRow, 18].Value = $"{(item.TotalTeacherVerifiedTHCS ?? default).GetPercent((item.ElevationOfRefHeightTHCS ?? default), 2)}%";

                //Tổng số Học viên xác thực thành công(HS + GV)
                var totalAccountVerifiedTHCS = (double)(item.TotalTeacherVerifiedTHCS ?? default) + (item.TotalStudentVerifiedTHCS ?? default);
                excelWorksheet.Cells[startRow, 19].Value = totalAccountVerifiedTHCS;
                excelWorksheet.Cells[startRow, 20].Value = $"{totalAccountVerifiedTHCS.GetPercent((item.TotalStudentDefaultTHCS ?? default), 2)}%";

                //SL HS đã hoàn thành PT
                excelWorksheet.Cells[startRow, 21].Value = item.TotalStudentPTCompleteTHCS;
                excelWorksheet.Cells[startRow, 22].Value = $"{(double)(item.TotalStudentPTCompleteTHCS ?? default).GetPercent((item.RegisterStudentTHCS ?? default), 2)}%";
                excelWorksheet.Cells[startRow, 23].Value = $"{(double)(item.TotalStudentPTCompleteTHCS ?? default).GetPercent((item.ElevationOfTerrainTHCS ?? default), 2)}%";

                excelWorksheet.Cells[startRow, 24].Value = item.TotalStudentPTProgressTHCS;
                excelWorksheet.Cells[startRow, 25].Value = $"{(double)(item.TotalStudentPTProgressTHCS ?? default).GetPercent((item.RegisterStudentTHCS ?? default), 2)}%";
                excelWorksheet.Cells[startRow, 26].Value = $"{(double)(item.TotalStudentPTProgressTHCS ?? default).GetPercent((item.ElevationOfTerrainTHCS ?? default), 2)}%";

                //SL GV đã hoàn thành PT
                excelWorksheet.Cells[startRow, 27].Value = item.TotalTeacherPTCompleteTHCS;
                excelWorksheet.Cells[startRow, 28].Value = $"{(double)(item.TotalTeacherPTCompleteTHCS ?? default).GetPercent((item.RegisterTeacherTHCS ?? default), 2)}%";
                excelWorksheet.Cells[startRow, 29].Value = $"{(double)(item.TotalTeacherPTCompleteTHCS ?? default).GetPercent((item.ElevationOfRefHeightTHCS ?? default), 2)}%";

                excelWorksheet.Cells[startRow, 30].Value = item.TotalTeacherPTProgressTHCS;
                excelWorksheet.Cells[startRow, 31].Value = $"{(double)(item.TotalTeacherPTProgressTHCS ?? default).GetPercent((item.RegisterTeacherTHCS ?? default), 2)}%";
                excelWorksheet.Cells[startRow, 32].Value = $"{(double)(item.TotalTeacherPTProgressTHCS ?? default).GetPercent((item.ElevationOfRefHeightTHCS ?? default), 2)}%";

                var totalAccountPTCompleteTHCS = (double)(item.TotalStudentPTCompleteTHCS ?? default) + (item.TotalTeacherPTCompleteTHCS ?? default);
                var totalAccountPTProgressTHCS = (double)(item.TotalStudentPTProgressTHCS ?? default) + (item.TotalTeacherPTProgressTHCS ?? default);

                excelWorksheet.Cells[startRow, 33].Value = totalAccountPTCompleteTHCS;
                excelWorksheet.Cells[startRow, 34].Value = $"{totalAccountPTCompleteTHCS.GetPercent((item.TotalStudentTHCS ?? default), 2)}%";
                excelWorksheet.Cells[startRow, 35].Value = $"{totalAccountPTCompleteTHCS.GetPercent((item.TotalStudentDefaultTHCS ?? default), 2)}%";

                excelWorksheet.Cells[startRow, 36].Value = totalAccountPTProgressTHCS;
                excelWorksheet.Cells[startRow, 37].Value = $"{totalAccountPTProgressTHCS.GetPercent((item.TotalStudentTHCS ?? default), 2)}%";
                excelWorksheet.Cells[startRow, 38].Value = $"{totalAccountPTProgressTHCS.GetPercent((item.TotalStudentDefaultTHCS ?? default), 2)}%";

                excelWorksheet.Cells[startRow, 39].Value = item.TotalStudentLearnProgressTHCS;
                excelWorksheet.Cells[startRow, 40].Value = $"{(double)(item.TotalStudentLearnProgressTHCS ?? default).GetPercent((item.ElevationOfTerrainTHCS ?? default), 2)}%";

                excelWorksheet.Cells[startRow, 41].Value = item.TotalTeacherLearnProgressTHCS;
                excelWorksheet.Cells[startRow, 42].Value = $"{(double)(item.TotalTeacherLearnProgressTHCS ?? default).GetPercent((item.ElevationOfRefHeightTHCS ?? default), 2)}%";

                var totalAccountLearnTHCS = (double)(item.TotalTeacherLearnProgressTHCS ?? default) + (item.TotalStudentLearnProgressTHCS ?? default);
                excelWorksheet.Cells[startRow, 43].Value = totalAccountLearnTHCS;
                excelWorksheet.Cells[startRow, 44].Value = $"{totalAccountLearnTHCS.GetPercent((item.TotalStudentDefaultTHCS ?? default), 2)}%";
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

                BorderRow(excelWorksheet, startRow, "A", "AR");
                var totalSchoolOtherDefault = (double)(item.TotalSchoolDefault ?? default) - (item.TotalSchoolTHCSDefault ?? default);
                var elevationOfTerrainOther = (double)(item.ElevationOfTerrain ?? default) - (item.ElevationOfTerrainTHCS ?? default);
                var elevationOfRefHeightOther = (double)(item.ElevationOfRefHeight ?? default) - (item.ElevationOfRefHeightTHCS ?? default);
                var totalAccountDefaultOther = (double)(item.TotalStudentDefault ?? default) - (item.TotalStudentDefaultTHCS ?? default);
                var totalAccountRegisterOther = (double)(item.TotalStudent ?? default) - (item.TotalStudentTHCS ?? default);
                var totalSchoolOther = (double)(item.TotalSchool ?? default) - (item.TotalSchoolTHCS ?? default);
                var registerStudentOther = (double)(item.RegisterStudent ?? default) - (item.RegisterStudentTHCS ?? default);
                var registerTeacherOther = (double)(item.RegisterTeacher ?? default) - (item.RegisterTeacherTHCS ?? default);

                var totalStudentVerifiedOther = (double)(item.TotalStudentVerified ?? default) - (item.TotalStudentVerifiedTHCS ?? default);
                var totalTeacherVerifiedOther = (double)(item.TotalTeacherVerified ?? default) - (item.TotalTeacherVerifiedTHCS ?? default);
                var totalAccountVerifiedOther = totalTeacherVerifiedOther + totalStudentVerifiedOther;

                var totalTeacherPTCompleteOther = (double)(item.TotalTeacherPTComplete ?? default) - (item.TotalTeacherPTCompleteTHCS ?? default);
                var totalTeacherPTProgressOther = (double)(item.TotalTeacherPTProgress ?? default) - (item.TotalTeacherPTProgressTHCS ?? default);
                var totalStudentPTCompleteOther = (double)(item.TotalStudentPTComplete ?? default) - (item.TotalStudentPTCompleteTHCS ?? default);
                var totalStudentPTProgressOther = (double)(item.TotalStudentPTProgress ?? default) - (item.TotalStudentPTProgressTHCS ?? default);

                var totalAccountPTCompleteOther = totalStudentPTCompleteOther + totalTeacherPTCompleteOther;
                var totalAccountPTProgressOther = totalStudentPTProgressOther + totalTeacherPTProgressOther;

                var totalStudentLearnProgressOther = (double)(item.TotalStudentLearnProgress ?? default) - (item.TotalStudentLearnProgressTHCS ?? default);
                var totalTeacherLearnProgressOther = (double)(item.TotalTeacherLearnProgress ?? default) - (item.TotalTeacherLearnProgressTHCS ?? default);
                var totalAccountLearnProgressOther = totalStudentLearnProgressOther + totalTeacherLearnProgressOther;

                excelWorksheet.Cells[startRow, 2].Value = item.DistrictName;
                excelWorksheet.Cells[startRow, 3].Value = totalSchoolOtherDefault;
                excelWorksheet.Cells[startRow, 4].Value = elevationOfTerrainOther;
                excelWorksheet.Cells[startRow, 5].Value = elevationOfRefHeightOther;
                excelWorksheet.Cells[startRow, 6].Value = totalAccountDefaultOther;
                excelWorksheet.Cells[startRow, 7].Value = totalSchoolOther;

                excelWorksheet.Cells[startRow, 8].Value = $"{totalSchoolOther.GetPercent(totalSchoolOtherDefault, 2)}%";
                excelWorksheet.Cells[startRow, 9].Value = registerStudentOther;
                excelWorksheet.Cells[startRow, 10].Value = $"{registerStudentOther.GetPercent(elevationOfTerrainOther, 2)}%";
                excelWorksheet.Cells[startRow, 11].Value = registerTeacherOther;
                excelWorksheet.Cells[startRow, 12].Value = $"{registerTeacherOther.GetPercent(elevationOfRefHeightOther, 2)}%";
                excelWorksheet.Cells[startRow, 13].Value = totalAccountRegisterOther;
                excelWorksheet.Cells[startRow, 14].Value = $"{totalAccountRegisterOther.GetPercent(totalAccountDefaultOther, 2)}%";

                excelWorksheet.Cells[startRow, 15].Value = totalStudentVerifiedOther;
                excelWorksheet.Cells[startRow, 16].Value = $"{totalStudentVerifiedOther.GetPercent(elevationOfTerrainOther, 2)}%";
                excelWorksheet.Cells[startRow, 17].Value = totalTeacherVerifiedOther;
                excelWorksheet.Cells[startRow, 18].Value = $"{totalTeacherVerifiedOther.GetPercent(elevationOfRefHeightOther, 2)}%";
                excelWorksheet.Cells[startRow, 19].Value = totalAccountVerifiedOther;
                excelWorksheet.Cells[startRow, 20].Value = $"{totalAccountVerifiedOther.GetPercent(totalAccountDefaultOther, 2)}%";

                excelWorksheet.Cells[startRow, 21].Value = totalStudentPTCompleteOther;
                excelWorksheet.Cells[startRow, 22].Value = $"{totalStudentPTCompleteOther.GetPercent(registerStudentOther, 2)}%";
                excelWorksheet.Cells[startRow, 23].Value = $"{totalStudentPTCompleteOther.GetPercent(elevationOfTerrainOther, 2)}%";
                excelWorksheet.Cells[startRow, 24].Value = totalStudentPTProgressOther;
                excelWorksheet.Cells[startRow, 25].Value = $"{totalStudentPTProgressOther.GetPercent(registerStudentOther, 2)}%";
                excelWorksheet.Cells[startRow, 26].Value = $"{totalStudentPTProgressOther.GetPercent(elevationOfTerrainOther, 2)}%";

                excelWorksheet.Cells[startRow, 27].Value = totalTeacherPTCompleteOther;
                excelWorksheet.Cells[startRow, 28].Value = $"{totalTeacherPTCompleteOther.GetPercent(registerTeacherOther, 2)}%";
                excelWorksheet.Cells[startRow, 29].Value = $"{totalTeacherPTCompleteOther.GetPercent(elevationOfRefHeightOther, 2)}%";
                excelWorksheet.Cells[startRow, 30].Value = totalTeacherPTProgressOther;
                excelWorksheet.Cells[startRow, 31].Value = $"{totalTeacherPTProgressOther.GetPercent(registerTeacherOther, 2)}%";
                excelWorksheet.Cells[startRow, 32].Value = $"{totalTeacherPTProgressOther.GetPercent(elevationOfRefHeightOther, 2)}%";

                excelWorksheet.Cells[startRow, 33].Value = totalAccountPTCompleteOther;
                excelWorksheet.Cells[startRow, 34].Value = $"{totalAccountPTCompleteOther.GetPercent(totalAccountRegisterOther, 2)}%";
                excelWorksheet.Cells[startRow, 35].Value = $"{totalAccountPTCompleteOther.GetPercent(totalAccountDefaultOther, 2)}%";
                excelWorksheet.Cells[startRow, 36].Value = totalAccountPTProgressOther;
                excelWorksheet.Cells[startRow, 37].Value = $"{totalAccountPTProgressOther.GetPercent(totalAccountRegisterOther, 2)}%";
                excelWorksheet.Cells[startRow, 38].Value = $"{totalAccountPTProgressOther.GetPercent(totalAccountDefaultOther, 2)}%";

                excelWorksheet.Cells[startRow, 39].Value = totalStudentLearnProgressOther;
                excelWorksheet.Cells[startRow, 40].Value = $"{totalStudentLearnProgressOther.GetPercent(elevationOfTerrainOther, 2)}%";
                excelWorksheet.Cells[startRow, 41].Value = totalTeacherLearnProgressOther;
                excelWorksheet.Cells[startRow, 42].Value = $"{totalTeacherLearnProgressOther.GetPercent(elevationOfRefHeightOther, 2)}%";
                excelWorksheet.Cells[startRow, 43].Value = totalAccountLearnProgressOther;
                excelWorksheet.Cells[startRow, 44].Value = $"{totalAccountLearnProgressOther.GetPercent(totalAccountDefaultOther, 2)}%";

                startRow++; // Di chuyển xuống dòng tiếp theo

                if (exportSummaryReports.IndexOf(item) == (exportSummaryReports.Count - 1))
                {
                    var totalSchoolTotal = exportSummaryReports.Sum(x => (double)(x.TotalSchool ?? 0));
                    var totalSchoolDefaultTotal = exportSummaryReports.Sum(x => (double)(x.TotalSchoolDefault ?? 0));
                    var registerStudentTotal = exportSummaryReports.Sum(x => (double)(x.RegisterStudent ?? 0));
                    var registerTeacherTotal = exportSummaryReports.Sum(x => (double)(x.RegisterTeacher ?? 0));
                    var elevationOfTerrainTotal = exportSummaryReports.Sum(x => (double)(x.ElevationOfTerrain ?? 0));
                    var elevationOfRefHeight = exportSummaryReports.Sum(x => (double)(x.ElevationOfRefHeight ?? 0));
                    var totalStudentTotal = exportSummaryReports.Sum(x => (double)(x.TotalStudent ?? 0));
                    var totalStudentDefaultTotal = exportSummaryReports.Sum(x => (double)(x.TotalStudentDefault ?? 0));

                    var totalStudentVerified = exportSummaryReports.Sum(x => (double)(x.TotalStudentVerified ?? 0));
                    var totalTeacherVerified = exportSummaryReports.Sum(x => (double)(x.TotalTeacherVerified ?? 0));
                    var totalAccountVerified = exportSummaryReports.Sum(x => (double)(x.TotalStudentVerified ?? 0) + (x.TotalTeacherVerified ?? 0));

                    var totalStudentPTComplete = exportSummaryReports.Sum(x => (double)(x.TotalStudentPTComplete ?? 0));
                    var totalTeacherPTComplete = exportSummaryReports.Sum(x => (double)(x.TotalTeacherPTComplete ?? 0));
                    var totalAccountPTComplete = exportSummaryReports.Sum(x => (double)(x.TotalStudentPTComplete ?? 0) + (x.TotalTeacherPTComplete ?? 0));

                    var totalStudentPTProgress = exportSummaryReports.Sum(x => (double)(x.TotalStudentPTProgress ?? 0));
                    var totalTeacherPTProgress = exportSummaryReports.Sum(x => (double)(x.TotalTeacherPTProgress ?? 0));
                    var totalAccountPTProgress = exportSummaryReports.Sum(x => (double)(x.TotalStudentPTProgress ?? 0) + (x.TotalTeacherPTProgress ?? 0));

                    var totalStudentLearnProgress = exportSummaryReports.Sum(x => (double)(x.TotalStudentLearnProgress ?? 0));
                    var totalTeacherLearnProgress = exportSummaryReports.Sum(x => (double)(x.TotalTeacherLearnProgress ?? 0));
                    var totalAccountLearnProgress = exportSummaryReports.Sum(x => (double)(x.TotalStudentLearnProgress ?? 0) + (x.TotalTeacherLearnProgress ?? 0));

                    excelWorksheet.Cells[$"B{startRow}"].Value = "Tổng";
                    excelWorksheet.Cells[$"B{startRow}"].Style.Font.Bold = true;
                    excelWorksheet.Cells[$"A{startRow}:AR{startRow}"].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    excelWorksheet.Cells[$"A{startRow}:AR{startRow}"].Style.Fill.BackgroundColor.SetColor(Color.Yellow);
                    excelWorksheet.Cells[$"C{startRow}"].Value = totalSchoolDefaultTotal;
                    excelWorksheet.Cells[$"D{startRow}"].Value = elevationOfTerrainTotal;
                    excelWorksheet.Cells[$"E{startRow}"].Value = elevationOfRefHeight;
                    excelWorksheet.Cells[$"F{startRow}"].Value = totalStudentDefaultTotal;
                    excelWorksheet.Cells[$"G{startRow}"].Value = totalSchoolTotal;
                    excelWorksheet.Cells[$"H{startRow}"].Value = $"{totalSchoolTotal.GetPercent(totalSchoolDefaultTotal, 2)}%";
                    excelWorksheet.Cells[$"I{startRow}"].Value = registerStudentTotal;
                    excelWorksheet.Cells[$"J{startRow}"].Value = $"{registerStudentTotal.GetPercent(elevationOfTerrainTotal, 2)}%";
                    excelWorksheet.Cells[$"K{startRow}"].Value = registerTeacherTotal;
                    excelWorksheet.Cells[$"L{startRow}"].Value = $"{registerTeacherTotal.GetPercent(elevationOfRefHeight, 2)}%";
                    excelWorksheet.Cells[$"M{startRow}"].Value = totalStudentTotal;
                    excelWorksheet.Cells[$"N{startRow}"].Value = $"{totalStudentTotal.GetPercent(totalStudentDefaultTotal, 2)}%";

                    //New
                    excelWorksheet.Cells[$"O{startRow}"].Value = totalStudentVerified;
                    excelWorksheet.Cells[$"P{startRow}"].Value = $"{totalStudentVerified.GetPercent(elevationOfTerrainTotal, 2)}%";
                    excelWorksheet.Cells[$"Q{startRow}"].Value = totalTeacherVerified;
                    excelWorksheet.Cells[$"R{startRow}"].Value = $"{totalTeacherVerified.GetPercent(elevationOfRefHeight, 2)}%";
                    excelWorksheet.Cells[$"S{startRow}"].Value = totalAccountVerified;
                    excelWorksheet.Cells[$"T{startRow}"].Value = $"{totalAccountVerified.GetPercent(totalStudentDefaultTotal, 2)}%";

                    excelWorksheet.Cells[$"U{startRow}"].Value = totalStudentPTComplete;
                    excelWorksheet.Cells[$"V{startRow}"].Value = $"{totalStudentPTComplete.GetPercent(registerStudentTotal, 2)}%";
                    excelWorksheet.Cells[$"W{startRow}"].Value = $"{totalStudentPTComplete.GetPercent(elevationOfTerrainTotal, 2)}%";
                    excelWorksheet.Cells[$"X{startRow}"].Value = totalStudentPTProgress;
                    excelWorksheet.Cells[$"Y{startRow}"].Value = $"{totalStudentPTProgress.GetPercent(registerStudentTotal, 2)}%";
                    excelWorksheet.Cells[$"Z{startRow}"].Value = $"{totalStudentPTProgress.GetPercent(elevationOfTerrainTotal, 2)}%";

                    excelWorksheet.Cells[$"AA{startRow}"].Value = totalTeacherPTComplete;
                    excelWorksheet.Cells[$"AB{startRow}"].Value = $"{totalTeacherPTComplete.GetPercent(registerTeacherTotal, 2)}%";
                    excelWorksheet.Cells[$"AC{startRow}"].Value = $"{totalTeacherPTComplete.GetPercent(elevationOfRefHeight, 2)}%";
                    excelWorksheet.Cells[$"AD{startRow}"].Value = totalTeacherPTProgress;
                    excelWorksheet.Cells[$"AE{startRow}"].Value = $"{totalTeacherPTProgress.GetPercent(registerTeacherTotal, 2)}%";
                    excelWorksheet.Cells[$"AF{startRow}"].Value = $"{totalTeacherPTProgress.GetPercent(elevationOfRefHeight, 2)}%";

                    excelWorksheet.Cells[$"AG{startRow}"].Value = totalAccountPTComplete;
                    excelWorksheet.Cells[$"AH{startRow}"].Value = $"{totalAccountPTComplete.GetPercent(totalStudentTotal, 2)}%";
                    excelWorksheet.Cells[$"AI{startRow}"].Value = $"{totalAccountPTComplete.GetPercent(totalStudentDefaultTotal, 2)}%";
                    excelWorksheet.Cells[$"AJ{startRow}"].Value = totalAccountPTProgress;
                    excelWorksheet.Cells[$"AK{startRow}"].Value = $"{totalAccountPTProgress.GetPercent(totalStudentTotal, 2)}%";
                    excelWorksheet.Cells[$"AL{startRow}"].Value = $"{totalAccountPTProgress.GetPercent(totalStudentDefaultTotal, 2)}%";

                    excelWorksheet.Cells[$"AM{startRow}"].Value = totalStudentLearnProgress;
                    excelWorksheet.Cells[$"AN{startRow}"].Value = $"{totalStudentLearnProgress.GetPercent(elevationOfTerrainTotal, 2)}%";
                    excelWorksheet.Cells[$"AO{startRow}"].Value = totalTeacherLearnProgress;
                    excelWorksheet.Cells[$"AP{startRow}"].Value = $"{totalTeacherLearnProgress.GetPercent(elevationOfRefHeight, 2)}%";
                    excelWorksheet.Cells[$"AQ{startRow}"].Value = totalAccountLearnProgress;
                    excelWorksheet.Cells[$"AR{startRow}"].Value = $"{totalAccountLearnProgress.GetPercent(totalStudentDefaultTotal, 2)}%";
                    BorderRow(excelWorksheet, startRow, "A", "AR");
                }
            }
        }

        private static void FillDataDivision(ExcelWorksheet excelWorksheet, IList<ExportSummaryReportCommandModel> exportSummaryReports)
        {
            int startRow = 8;
            int countData = 0;
            int countLevelTHCS = 0;
            int levelTHCS = exportSummaryReports.Count(x => x.Level == 3);

            foreach (var item in exportSummaryReports.OrderBy(x => x.Level == 3 ? 0 : 1).ThenBy(x => x.Level).ThenByDescending(x => x.TotalStudentDefault))
            {
                countData++;
                countLevelTHCS++;

                BorderRow(excelWorksheet, startRow, "A", "AO");
                var totalPTComplete = (double)(item.TotalTeacherPTComplete ?? default) + (item.TotalStudentPTComplete ?? default);
                var totalPTProgress = (double)(item.TotalTeacherPTProgress ?? default) + (item.TotalStudentPTProgress ?? default);
                var sumAccountVerified = (double)(item.TotalTeacherVerified ?? default) + (item.TotalStudentVerified ?? default);

                excelWorksheet.Cells[startRow, 2].Value = item.SchoolName;
                excelWorksheet.Cells[startRow, 3].Value = item.ElevationOfTerrain;
                excelWorksheet.Cells[startRow, 4].Value = item.ElevationOfRefHeight;
                excelWorksheet.Cells[startRow, 5].Value = item.TotalStudentDefault;
                excelWorksheet.Cells[startRow, 6].Value = item.RegisterStudent;
                excelWorksheet.Cells[startRow, 7].Value = $"{((double)(item.RegisterStudent ?? default)).GetPercent((item.ElevationOfTerrain ?? default), 2)}%";
                excelWorksheet.Cells[startRow, 8].Value = item.RegisterTeacher;
                excelWorksheet.Cells[startRow, 9].Value = $"{(double)(item.RegisterTeacher ?? default).GetPercent((item.ElevationOfRefHeight ?? default), 2)}%";
                excelWorksheet.Cells[startRow, 10].Value = item.TotalStudent;
                excelWorksheet.Cells[startRow, 11].Value = $"{(double)(item.TotalStudent ?? default).GetPercent((item.TotalStudentDefault ?? default), 2)}%";

                // New
                excelWorksheet.Cells[startRow, 12].Value = item.TotalStudentVerified;
                excelWorksheet.Cells[startRow, 13].Value = $"{((double)(item.TotalStudentVerified ?? default)).GetPercent((item.ElevationOfTerrain ?? default), 2)}%";

                excelWorksheet.Cells[startRow, 14].Value = item.TotalTeacherVerified;
                excelWorksheet.Cells[startRow, 15].Value = $"{((double)(item.TotalTeacherVerified ?? default)).GetPercent((item.ElevationOfRefHeight ?? default), 2)}%";

                excelWorksheet.Cells[startRow, 16].Value = sumAccountVerified;
                excelWorksheet.Cells[startRow, 17].Value = $"{sumAccountVerified.GetPercent((item.TotalStudentDefault ?? default), 2)}%";

                excelWorksheet.Cells[startRow, 18].Value = item.TotalStudentPTComplete;
                excelWorksheet.Cells[startRow, 19].Value = $"{((double)(item.TotalStudentPTComplete ?? default)).GetPercent((item.RegisterStudent ?? default), 2)}%";
                excelWorksheet.Cells[startRow, 20].Value = $"{((double)(item.TotalStudentPTComplete ?? default)).GetPercent((item.ElevationOfTerrain ?? default), 2)}%";

                excelWorksheet.Cells[startRow, 21].Value = item.TotalStudentPTProgress;
                excelWorksheet.Cells[startRow, 22].Value = $"{((double)(item.TotalStudentPTProgress ?? default)).GetPercent((item.RegisterStudent ?? default), 2)}%";
                excelWorksheet.Cells[startRow, 23].Value = $"{((double)(item.TotalStudentPTProgress ?? default)).GetPercent((item.ElevationOfTerrain ?? default), 2)}%";

                excelWorksheet.Cells[startRow, 24].Value = item.TotalTeacherPTComplete;
                excelWorksheet.Cells[startRow, 25].Value = $"{((double)(item.TotalTeacherPTComplete ?? default)).GetPercent((item.RegisterTeacher ?? default), 2)}%";
                excelWorksheet.Cells[startRow, 26].Value = $"{((double)(item.TotalTeacherPTComplete ?? default)).GetPercent((item.ElevationOfRefHeight ?? default), 2)}%";

                excelWorksheet.Cells[startRow, 27].Value = item.TotalTeacherPTProgress;
                excelWorksheet.Cells[startRow, 28].Value = $"{((double)(item.TotalTeacherPTProgress ?? default)).GetPercent((item.RegisterTeacher ?? default), 2)}%";
                excelWorksheet.Cells[startRow, 29].Value = $"{((double)(item.TotalTeacherPTProgress ?? default)).GetPercent((item.ElevationOfRefHeight ?? default), 2)}%";

                excelWorksheet.Cells[startRow, 30].Value = totalPTComplete;
                excelWorksheet.Cells[startRow, 31].Value = $"{totalPTComplete.GetPercent((item.TotalStudent ?? default), 2)}%";
                excelWorksheet.Cells[startRow, 32].Value = $"{totalPTComplete.GetPercent((item.TotalStudentDefault ?? default), 2)}%";

                excelWorksheet.Cells[startRow, 33].Value = totalPTProgress;
                excelWorksheet.Cells[startRow, 34].Value = $"{totalPTProgress.GetPercent((item.TotalStudent ?? default), 2)}%";
                excelWorksheet.Cells[startRow, 35].Value = $"{totalPTProgress.GetPercent((item.TotalStudentDefault ?? default), 2)}%";

                excelWorksheet.Cells[startRow, 36].Value = item.TotalStudentLearnProgress;
                excelWorksheet.Cells[startRow, 37].Value = $"{((double)(item.TotalStudentLearnProgress ?? default)).GetPercent((item.ElevationOfTerrain ?? default), 2)}%";

                excelWorksheet.Cells[startRow, 38].Value = item.TotalTeacherLearnProgress;
                excelWorksheet.Cells[startRow, 39].Value = $"{((double)(item.TotalTeacherLearnProgress ?? default)).GetPercent((item.ElevationOfRefHeight ?? default), 2)}%";

                var totalLearn = (double)(item.TotalStudentLearnProgress ?? default) + (item.TotalTeacherLearnProgress ?? default);
                excelWorksheet.Cells[startRow, 40].Value = totalLearn;
                excelWorksheet.Cells[startRow, 41].Value = $"{totalLearn.GetPercent((item.TotalStudentDefault ?? default), 2)}%";

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