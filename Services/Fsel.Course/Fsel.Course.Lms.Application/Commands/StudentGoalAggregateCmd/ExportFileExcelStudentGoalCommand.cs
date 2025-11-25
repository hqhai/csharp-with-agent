// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Commands.StudentGoalAggregateCmd
{
    using Common.ActionResults;
    using Core.Base;
    using Domain.Enums;
    using Domain.IRepositories;
    using Domain.Models.EntityModels;
    using Domain.Models.QueryModels.StudentProgress;
    using MediatR;
    using OfficeOpenXml;
    using Queries.StudentGoalSummaryQuery;
    using Services.UserServices;
    using Shared.Constants;

    public class ExportFileExcelStudentGoalCommand : SearchStudentGoalAggregateQueryModel, IRequest<MethodResult<Stream>>
    {
        public EnumExportScope Scope { get; set; }
        public IList<Guid>? SelectedIds { get; set; }
    }

    public class ExportFileExcelStudentGoalCommandHandler : IRequestHandler<ExportFileExcelStudentGoalCommand, MethodResult<Stream>>
    {
        private readonly IStudentGoalAggregateRepository _studentGoalAggregateRepository;
        private readonly IStudentGoalSummaryRepository _studentGoalSummaryRepository;
        private readonly IUserService _userService;
        private readonly AuthContext _authContext;
        private readonly IMediator _mediator;

        public ExportFileExcelStudentGoalCommandHandler(IStudentGoalAggregateRepository studentGoalAggregateRepository,
            IStudentGoalSummaryRepository studentGoalSummaryRepository,
            IUserService userService,
            AuthContext authContext,
            IMediator mediator)
        {
            _studentGoalAggregateRepository = studentGoalAggregateRepository;
            _studentGoalSummaryRepository = studentGoalSummaryRepository;
            _userService = userService;
            _authContext = authContext;
            _mediator = mediator;
        }

        public async Task<MethodResult<Stream>> Handle(ExportFileExcelStudentGoalCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<Stream>();

            var dataResult = await _mediator.Send(new GetStudentGoalQuery()
            {
                CourseType = request.CourseType,
                SchoolId = request.SchoolId,
                ClassIdStr = request.ClassIdStr,
                CombinedProgress = request.CombinedProgress,
                StatusStudentGoal = request.StatusStudentGoal,
                ClassCampusCode = request.ClassCampusCode,
            }, cancellationToken);

            IEnumerable<StudentGoalAggregateModel> items = dataResult.Result;

            if (request.Scope == EnumExportScope.Selected
                && request.SelectedIds != null
                && request.SelectedIds.Any())
            {
                items = items.Where(x => request.SelectedIds.Contains(x.Id));
            }

            methodResult.Result = ExportExcelTemplate(request, items.ToList());

            return methodResult;
        }

        private Stream? ExportExcelTemplate(ExportFileExcelStudentGoalCommand request, IList<StudentGoalAggregateModel> dataResultResult)
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
                worksheet.Cells[startRow, 9].Value = item.StatusStudentGoal;
                worksheet.Cells[startRow, 10].Value = item.CourseType;
                worksheet.Cells[startRow, 11].Value = item.CourseLevel;

                count++;
                startRow++;
            }
        }
    }
}
