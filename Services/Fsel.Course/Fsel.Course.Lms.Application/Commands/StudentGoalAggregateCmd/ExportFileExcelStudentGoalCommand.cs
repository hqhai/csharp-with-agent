// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Commands.StudentGoalAggregateCmd
{
    using Common.ActionResults;
    using Common.Helpers;
    using Domain.Models.EntityModels;
    using Domain.Models.QueryModels.StudentProgress;
    using MediatR;
    using OfficeOpenXml;
    using Queries.StudentGoalSummaryQuery;
    using Shared.Constants;

    public class ExportFileExcelStudentGoalCommand : SearchStudentGoalAggregateQueryModel, IRequest<MethodResult<Stream>>
    {
        public bool IsSelected { get; set; } = false;
        public IList<Guid>? SelectedIds { get; set; }
    }

    public class ExportFileExcelStudentGoalCommandHandler : IRequestHandler<ExportFileExcelStudentGoalCommand, MethodResult<Stream>>
    {
        private readonly IMediator _mediator;

        public ExportFileExcelStudentGoalCommandHandler(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task<MethodResult<Stream>> Handle(ExportFileExcelStudentGoalCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<Stream>();

            var dataResult = await _mediator.Send(new GetStudentGoalQuery()
            {
                LevelId = request.LevelId,
                SchoolId = request.SchoolId,
                ClassIdStr = request.ClassIdStr,
                CombinedProgress = request.CombinedProgress,
                StatusStudentCampus = request.StatusStudentCampus,
                ClassCampusCode = request.ClassCampusCode,
            }, cancellationToken);

            IEnumerable<StudentGoalAggregateModel>? items = dataResult.Result;

            if (request.IsSelected && request.SelectedIds != null && request.SelectedIds.Any())
            {
                items = items.Where(x => request.SelectedIds.Contains(x.Id));
            }

            methodResult.Result = ExportExcelTemplate(request, items.ToList());

            return methodResult;
        }

        private Stream ExportExcelTemplate(ExportFileExcelStudentGoalCommand request, IList<StudentGoalAggregateModel> dataResultResult)
        {
            ArgumentNullException.ThrowIfNull(request);
            var memoryStream = new MemoryStream();
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            const string FilePath = ResourceSettings.StudentGoal;
            using (var templateStream = new FileStream(FilePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (var excelPackage = new ExcelPackage(templateStream))
            {
                var excelWorksheet = excelPackage.Workbook.Worksheets[0];
                FillData(excelWorksheet, dataResultResult);
                excelPackage.SaveAs(memoryStream);
            }

            memoryStream.Position = 0L;
            return memoryStream;
        }

        private static void FillData(ExcelWorksheet worksheet, IList<StudentGoalAggregateModel> dataResultResult)
        {
            int startRow = 2;
            int count = 1;

            foreach (var item in dataResultResult)
            {
                worksheet.Cells[startRow, 1].Value = count;
                worksheet.Cells[startRow, 2].Value = item.FullName;
                worksheet.Cells[startRow, 3].Value = item.StudentCampusCode;
                worksheet.Cells[startRow, 4].Value = item.ClassName;
                worksheet.Cells[startRow, 5].Value = item.ClassCampusCode;
                worksheet.Cells[startRow, 6].Value = item.TotalCompletedLessons + "/" + item.TotalTargetLessons;
                worksheet.Cells[startRow, 7].Value = item.CompletedLessons + "/" + item.LessonsPerWeek;
                worksheet.Cells[startRow, 8].Value = item.ConsecutiveBehindWeeks;
                worksheet.Cells[startRow, 9].Value = item.StatusStudentCampus!.GetDescription();
                worksheet.Cells[startRow, 10].Value = item.CourseType;
                worksheet.Cells[startRow, 11].Value = item.LevelId.HasValue ? item.LevelId.Value.ToString() : string.Empty;

                count++;
                startRow++;
            }
        }
    }
}
