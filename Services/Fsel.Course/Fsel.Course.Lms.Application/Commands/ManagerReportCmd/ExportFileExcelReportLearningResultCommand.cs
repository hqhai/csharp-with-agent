// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Commands.ManagerReportCmd
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Helpers;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels.ManagerReportModels;
    using Fsel.Course.Domain.Models.QueryModels.ManagerReports;
    using Fsel.Course.Lms.Application.Queries.ManagerReportQuery;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Helpers;
    using MediatR;
    using OfficeOpenXml;
    using OfficeOpenXml.Style;
    using static Fsel.Shared.Constants.ValueSettings;

    public class ExportFileExcelReportLearningResultCommand
        : SearchReportLearningResultQueryModel, IRequest<MethodResult<Stream>>
    {
        public ExportFileExcelReportLearningResultCommand()
        {
            ManagerReportType = EnumManagerReportType.ReportLearningResults;
        }
    }

    public class ExportFileExcelReportLearningResultCommandHandler
        : IRequestHandler<ExportFileExcelReportLearningResultCommand, MethodResult<Stream>>
    {
        private readonly IUserService _userService;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IMediator _mediator;

        public ExportFileExcelReportLearningResultCommandHandler(
            IUserService userService,
            ICategoryRepository categoryRepository,
            IMediator mediator)
        {
            _userService = userService;
            _categoryRepository = categoryRepository;
            _mediator = mediator;
        }

        public async Task<MethodResult<Stream>> Handle(
            ExportFileExcelReportLearningResultCommand request,
            CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            var learningResult = await _mediator.Send(new GetReportLearningResultQuery(request), cancellationToken);
            var overallResult = await _mediator.Send(new GetOverallReportLearningResultQuery(request), cancellationToken);

            var userProfile = await _userService.GetUserProfileAsync();
            var schoolName = userProfile.Content?.Result?.SchoolName ?? "Trường";

            var program =
                await _categoryRepository.GetProgramLevelsAsync(request.ProgramId, cancellationToken);

            var stream = ExportExcelTemplate(
                request,
                learningResult.Result,
                overallResult.Result,
                program,
                schoolName,
                overallResult.Result?.MaxUnitCount ?? 0);

            return new MethodResult<Stream> { Result = stream };
        }

        #region Excel Export

        private static Stream ExportExcelTemplate(
            ExportFileExcelReportLearningResultCommand request,
            IList<LearningResultReportModel>? learningResults,
            OverallReportLearningResultModel? overallReport,
            Category? program,
            string schoolName,
            int maxUnit)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            var stream = new MemoryStream();
            using var package = new ExcelPackage();

            var sheet = package.Workbook.Worksheets.Add("Report");

            DesignExcelTemplate(
                sheet,
                request,
                program,
                schoolName,
                overallReport,
                maxUnit);

            if (learningResults != null && learningResults.Any())
            {
                FillLearningProgressData(sheet, maxUnit, learningResults);
            }

            package.SaveAs(stream);
            stream.Position = 0;
            return stream;
        }

        private static void FillLearningProgressData(ExcelWorksheet worksheet, int maxUnit, IList<LearningResultReportModel> learningProgressReports)
        {
            int startRow = 10;

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
                worksheet.Cells[startRow, 7].Value = item.LevelName;
                worksheet.Cells[startRow, 8].Value = ConvertPercent(item.OverallPercent, "-");
                var index = 9;
                var ind = maxUnit + 9;
                foreach (var data in item.OverallModules)
                {
                    worksheet.Cells[startRow, index].Value = ConvertPercent(data.Percent, "-");
                    index++;
                }

                worksheet.Cells[startRow, ind].Value = processDateStr;
                worksheet.Cells[startRow, ind + 1].Value = item.StatusStr;
                worksheet.Cells[startRow, ind + 2].Value = expiredDateStr;
                startRow++;
            }
        }

        private static string ConvertPercent(double? percent, string pattern = "0 %")
        {
            return percent.HasValue ? $"{percent.Value} %" : pattern;
        }

        private static void DesignExcelTemplate(
            ExcelWorksheet sheet,
            ExportFileExcelReportLearningResultCommand request,
            Category? program,
            string schoolName,
            OverallReportLearningResultModel? overallReport,
            int maxUnit)
        {
            BuildTitle(sheet, program);
            BuildBasicInfo(sheet, request, schoolName, program, overallReport);
            BuildLevelSummary(sheet, overallReport);
            BuildUnitSummary(sheet, overallReport, maxUnit);
            BuildTableHeader(sheet, maxUnit);
            BuildFilterLabelWithValue(sheet, request, program);
        }

        #endregion Excel Export

        #region Excel Sections

        private static void BuildTitle(ExcelWorksheet sheet, Category? program)
        {
            sheet.Cells["A1:Z1"].Merge = true;
            sheet.Cells["A1"].Value = $"BÁO CÁO KẾT QUẢ HỌC TẬP ({program?.Name})";
            sheet.Cells["A1"].Style.Font.Bold = true;
            sheet.Cells["A1"].Style.Font.Size = 14;
            sheet.Cells["A1"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
        }

        private static void BuildBasicInfo(
            ExcelWorksheet sheet,
            ExportFileExcelReportLearningResultCommand request,
            string schoolName,
            Category? program,
            OverallReportLearningResultModel? overallReport)
        {
            string[] leftLabels =
            {
                "Trường:",
                "Tổng số học sinh:",
                "Số lượng học sinh theo khóa học:",
                "Điểm tổng trung bình:",
                "Trung bình kết quả học tập:"
            };

            for (int i = 0; i < leftLabels.Length; i++)
            {
                sheet.Cells[i + 2, 1].Value = leftLabels[i];
                sheet.Cells[i + 2, 1].Style.Font.Bold = true;
            }

            sheet.Cells["C2"].Value = schoolName;
            sheet.Cells["C3"].Value = overallReport?.TotalStudent;
            sheet.Cells["C5"].Value = $"{overallReport?.OverallAvgPercent} %";

            ApplyHeaderStyle(sheet.Cells["C2"]);
            ApplyHeaderStyle(sheet.Cells["C3"]);
            ApplyHeaderStyle(sheet.Cells["C5"]);

            var levels = program?.Levels?
                .Where(x => request.LevelIds?.Contains(x.Id) == true)
                .Select(x => x.Name)
                .ToList();

            sheet.Cells["D2"].Value = request.ListLearningStatus;
            sheet.Cells["E2"].Value = request.ListSchoolGrade;
            sheet.Cells["F2"].Value = request.ListSchoolClass;
            sheet.Cells["G2"].Value = levels?.Any() == true ? string.Join(",", levels) : null;
            sheet.Cells["H2"].Value = request.OverallScores != null && request.OverallScores.Any() ? string.Join(",", request.OverallScores.Select(x => x.GetDescription())) : null;

            sheet.Cells["I2"].Value = request.EndDate?
                .ToString("dd-MM-yyyy HH:mm", CultureInfo.CurrentCulture);

            sheet.Cells["J2"].Value = DateTime.UtcNow
                .ConvertTimeFromUtc(EnumCountryKey.Vietnam)
                .ToString("dd-MM-yyyy HH:mm", CultureInfo.CurrentCulture);

            for (int col = 4; col <= 10; col++)
            {
                ApplyHeaderStyle(sheet.Cells[2, col]);
            }
        }

        private static void BuildLevelSummary(
            ExcelWorksheet sheet,
            OverallReportLearningResultModel? overallReport)
        {
            if (overallReport?.CourseLevelProgresses?.Any() != true)
            {
                return;
            }

            int row = 4;
            int col = 3;

            foreach (var item in overallReport.CourseLevelProgresses)
            {
                sheet.Cells[row, col].Value = $"{item.LevelName}: {item.TotalStudent} hs";
                sheet.Cells[row, col].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                sheet.Cells[row, col].Style.WrapText = true;
                sheet.Column(col).Width = 14;
                col++;
            }

            sheet.Cells[row, 2, row, col].Style.Font.Bold = true;
        }

        private static void BuildUnitSummary(
            ExcelWorksheet sheet,
            OverallReportLearningResultModel? overallReport,
            int maxUnit)
        {
            int startRow = 6;
            int col = 3;

            for (int unit = 1; unit <= maxUnit; unit++)
            {
                var module = overallReport?.OverallModules?
                    .FirstOrDefault(x => x.DisplayOrder == unit);

                sheet.Cells[startRow, col].Value = $"Unit {unit}";
                sheet.Cells[startRow + 1, col].Value = $"{module?.TotalStudent ?? 0} hs";
                sheet.Cells[startRow + 2, col].Value = $"{module?.Percent ?? 0}%";

                var range = sheet.Cells[startRow, col, startRow + 2, col];
                range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                range.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                range.Style.WrapText = true;

                sheet.Column(col).Width = 10;
                col++;
            }

            sheet.Cells[6, 2, 8, 2 + maxUnit].Style.Font.Bold = true;
        }

        private static void BuildFilterLabelWithValue(
        ExcelWorksheet sheet,
        ExportFileExcelReportLearningResultCommand request,
        Category? program)
        {
            var levels = program?.Levels?
                .Where(x => request.LevelIds != null && request.LevelIds.Contains(x.Id))
                .Select(x => x.Code)
                .ToList()
                ?? new List<string?>();

            string learningStatusStr = string.Join(
                ", ",
                (request.ListLearningStatus?.ToList<EnumLearningStatus>() ?? new List<EnumLearningStatus>())
                    .Select(x => x.GetDescription()));

            string overallScoreStr = string.Join(
                ", ",
                (request.OverallScores ?? new List<EnumOverallScore>())
                    .Select(x => x.GetDescription()));

            string endDateStr = request.EndDate.HasValue
                ? request.EndDate.Value.ToString("dd-MM-yyyy HH:mm", CultureInfo.CurrentCulture)
                : string.Empty;

            string nowStr = DateTime.UtcNow
                .ConvertTimeFromUtc(EnumCountryKey.Vietnam)
                .ToString("dd-MM-yyyy HH:mm", CultureInfo.CurrentCulture);

            string[] labelRows =
            {
                $"Trạng thái : {learningStatusStr}",
                $"Khối: {request.ListSchoolGrade}",
                $"Lớp: {request.ListSchoolClass}",
                $"Khóa học: {(levels.Any() ? string.Join(", ", levels) : string.Empty)}",
                $"Điểm tổng trung bình: {overallScoreStr}",
                $"Đến ngày: {endDateStr}",
                $"Ngày: {nowStr}"
            };

            for (int i = 0; i < labelRows.Length; i++)
            {
                var col = i + 4;
                var cell = sheet.Cells[2, col]; // D2 -> J2
                cell.Value = labelRows[i];
                cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                sheet.Column(col).Width = 26;
            }
        }

        private static void BuildTableHeader(ExcelWorksheet sheet, int maxUnit)
        {
            int headerRow = 9;
            int col = 1;

            string[] fixedHeaders =
            {
                "Họ và tên",
                "UserName",
                "Số điện thoại",
                "Email",
                "Khối",
                "Lớp",
                "Khóa học",
                "Điểm tổng kết\ntrung bình"
            };

            foreach (var header in fixedHeaders)
            {
                sheet.Cells[headerRow, col].Value = header;
                sheet.Cells[headerRow, col].Style.WrapText = true;
                sheet.Column(col).Width = col == 1 ? 32 : 18;
                col++;
            }

            for (int unit = 1; unit <= maxUnit; unit++)
            {
                sheet.Cells[headerRow, col].Value = $"Unit {unit}\noverall";
                sheet.Cells[headerRow, col].Style.WrapText = true;
                sheet.Column(col).Width = 12;
                col++;
            }
            string[] leftLabels =
            {
                "Ngày bắt đầu học",
                "Trạng thái",
                "Ngày hết hạn"
            };

            for (int i = 0; i < leftLabels.Length; i++)
            {
                var rangeData = sheet.Cells[headerRow, col];
                rangeData.Value = leftLabels[i];
                rangeData.Style.Font.Bold = true;
                rangeData.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                rangeData.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                rangeData.Style.WrapText = true;
                sheet.Column(col).Width = 24;
                col++;
            }

            var range = sheet.Cells[headerRow, 1, headerRow, col - 1];
            range.Style.Font.Bold = true;
            range.Style.Fill.PatternType = ExcelFillStyle.Solid;
            range.Style.Fill.BackgroundColor
                .SetColor(System.Drawing.Color.FromArgb(0, 102, 153));
            range.Style.Font.Color.SetColor(System.Drawing.Color.White);
            range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            sheet.Row(headerRow).Height = 44;
        }

        #endregion Excel Sections

        #region Helpers

        private static void ApplyHeaderStyle(ExcelRange cell)
        {
            cell.Style.Font.Bold = true;
            cell.Style.Font.Size = 12;
            cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
        }

        #endregion Helpers
    }
}
