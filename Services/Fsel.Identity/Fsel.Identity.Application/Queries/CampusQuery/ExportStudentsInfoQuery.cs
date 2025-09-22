// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Queries.CampusQuery
{
    using System.Collections.Concurrent;
    using System.Globalization;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Models.ShareModels.CampusModel;
    using MediatR;
    using OfficeOpenXml;

    public class ExportStudentsInfoQuery : IRequest<MethodResult<Stream>>
    {
        public IList<Guid>? StudentIds { get; set; }
    }

    public class ExportStudentsInfoQueryHandler : IRequestHandler<ExportStudentsInfoQuery, MethodResult<Stream>>
    {
        private readonly IMediator _mediator;

        public ExportStudentsInfoQueryHandler(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task<MethodResult<Stream>> Handle(ExportStudentsInfoQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<Stream>();

            var searchStudentQuery = new SearchStudentsQuery()
            {
                StudentIds = request.StudentIds
            };

            searchStudentQuery.SetIsQueryAll(true);

            var searchResult = await _mediator.Send(searchStudentQuery, cancellationToken).ConfigureAwait(false);
            if (!searchResult.IsOK)
            {
                methodResult.AddError(searchResult.ErrorMessages);
                return methodResult;
            }

            var students = searchResult.Result?.Items;

            if (students == null || students.Count == 0)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                return methodResult;
            }

            methodResult.Result = ExportExcelTemplate(students);
            return methodResult;
        }

        public static Stream ExportExcelTemplate(IList<StudentCampusModel> students)
        {
            ArgumentNullException.ThrowIfNull(students);
            var memoryStream = new MemoryStream();

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            var cultureInfo = CultureInfo.InvariantCulture;
            using var excelPackage = new ExcelPackage(new FileInfo(ResourceSettings.ReportStudentsByAdminSchool));
            var excelWorksheet = excelPackage.Workbook.Worksheets[0];
            int startRow = 2;

            // ConcurrentBag để lưu dữ liệu trung gian, hỗ trợ đa luồng
            var dataBag = new ConcurrentBag<(int row, object[] values)>();

            Parallel.ForEach(students, (item, state, index) =>
            {
                object[] rowValues = new object[]
                {
                    item.FullName ?? string.Empty,
                    item.UserName ?? string.Empty,
                    item.Email ?? string.Empty,
                    item.PhoneNumber ?? string.Empty
                };

                // Lưu dữ liệu vào ConcurrentBag (thay vì ghi vào Excel ngay)
                dataBag.Add((startRow + (int)index, rowValues));
            });

            // Sắp xếp lại dữ liệu theo thứ tự hàng
            var sortedData = dataBag.OrderBy(x => x.row);

            // Ghi tất cả dữ liệu vào Excel trong một lần
            foreach (var (row, values) in sortedData)
            {
                for (int col = 1; col <= values.Length; col++)
                {
                    excelWorksheet.Cells[row, col].Value = values[col - 1];
                }
            }

            excelPackage.SaveAs(memoryStream);
            memoryStream.Position = 0L;
            return memoryStream;
        }
    }
}
