namespace Fsel.Identity.Application.Queries.AdminQuery
{
    using System.Collections.Concurrent;
    using System.Globalization;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Identity.Domain.Models.EntityModels;
    using Fsel.Identity.Domain.Models.QueryModels.Students;
    using Fsel.Shared.Constants;
    using MediatR;
    using OfficeOpenXml;

    public class ExportStudentsByAdminSchoolQuery : SearchStudentsQueryModel, IRequest<MethodResult<Stream>>
    {
    }

    public class ExportStudentsByAdminSchoolQueryHandler : IRequestHandler<ExportStudentsByAdminSchoolQuery, MethodResult<Stream>>
    {
        private readonly IMediator _mediator;

        public ExportStudentsByAdminSchoolQueryHandler(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task<MethodResult<Stream>> Handle(ExportStudentsByAdminSchoolQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<Stream>();

            var searchStudentQuery = new SearchStudentsQuery()
            {
                Keyword = request.Keyword,
                SchoolName = request.SchoolName,
                ListSchoolClass = request.ListSchoolClass,
                ListSchoolGrade = request.ListSchoolGrade
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

        public static Stream ExportExcelTemplate(IList<StudentSearchAdminModel> students)
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
                    item.PhoneNumber ?? string.Empty,
                    item.Email ?? string.Empty,
                    item.Birthday.HasValue ? item.Birthday.Value.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture) : string.Empty,
                    item.Grade ?? string.Empty,
                    item.Class ?? string.Empty,
                    item.UserName ?? string.Empty,
                    item.PasswordDefault ?? string.Empty,
                    !item.CreatedDate.HasValue ? string.Empty : item.CreatedDate.Value.AddHours(7).ToString("dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture)
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
