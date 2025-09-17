// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Commands.Campus
{
    using System.Drawing;
    using System.Globalization;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Common.Helpers;
    using Fsel.Common.Models.Excels;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Core.Base.Managers;
    using Fsel.Identity.Application.Services.LmsCourseService;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Identity.Domain.IRepositories;
    using Fsel.Identity.Domain.Models.CommandModels.Campus;
    using Fsel.Shared.Models.ShareModels;
    using Fsel.Shared.Models.ShareModels.CampusModel;
    using Kros.Extensions;
    using MediatR;
    using Microsoft.EntityFrameworkCore;
    using OfficeOpenXml;
    using OfficeOpenXml.Style;

    public class AddStudentsToCurriculumCommand : BaseImportCommandModel, IRequest<MethodResult<Stream>>
    {
        public Guid CurriculumId { get; set; }
    }

    public class AddStudentsToCurriculumCommandHandler : IRequestHandler<AddStudentsToCurriculumCommand, MethodResult<Stream>>
    {
        private readonly IHumanRepository _humanRepository;
        private readonly IStudentRepository _studentRepository;
        private readonly UserManager<User> _userManager;
        private readonly ILmsCourseService _lmsCourseService;

        private const string ErrorMessage = "Thông báo lỗi";
        private const string EmptyFullName = "Chưa nhập Họ và Tên";
        private const string EmptyEmail = "Chưa nhập Email";
        private const string DuplicateData = "Tên tài khoản trùng lặp trong file tải lên";
        private const string DataNotExist = "Tên tài khoản không tồn tại trên hệ thống";
        private const string ErrorTemplate = "Template bị sai, kiểm tra lại tên cột, bạn cần download template ở nút Tải Template mẫu";
        private const string DataError = "Dữ liệu bị trống hoặc sai định dạng";
        private const string InvalidEmail = "Email sai định dạng";

        public AddStudentsToCurriculumCommandHandler(IHumanRepository humanRepository, IStudentRepository studentRepository, UserManager<User> userManager, ILmsCourseService lmsCourseService)
        {
            _humanRepository = humanRepository;
            _studentRepository = studentRepository;
            _userManager = userManager;
            _lmsCourseService = lmsCourseService;
        }

        public async Task<MethodResult<Stream>> Handle(AddStudentsToCurriculumCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<Stream>();

            if (request.FormFile == null)
            {
                methodResult.AddError(nameof(EnumSystemErrorCode.ImportFileRequired));
                return methodResult;
            }

            Action<ExcelWorksheet, Dictionary<string, int?>?, IList<ValidateExcelModel>> errorHandlerAction = (worksheet, columnIndexes, errors) =>
            {
                worksheet.Cells[1, 3].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                worksheet.Cells[1, 3].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                worksheet.Cells[1, 3].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                worksheet.Cells[1, 3].Style.Border.Right.Style = ExcelBorderStyle.Thin;
                worksheet.Cells[1, 3].Value = ErrorMessage;
                worksheet.Cells[1, 3].Style.Font.Bold = true;
                foreach (var error in errors.GroupBy(x => x.RowIndex).Select(x => x).OrderBy(x => x.Key))
                {
                    var row = error.Key;

                    int lastColumn = 3;

                    // Tạo biến lưu trữ dữ liệu dòng hiện tại
                    List<object?> rowData = new List<object?>();
                    // Lưu dữ liệu của dòng vào biến rowData
                    for (int col = 1; col <= worksheet.Dimension.End.Column; col++)
                    {
                        rowData.Add(worksheet.Cells[row, col].Value);
                    }

                    int targetRow = 2;
                    worksheet.DeleteRow(row, 1);
                    worksheet.InsertRow(targetRow, 1);

                    // Gắn lại dữ liệu đã lưu vào dòng mới
                    for (int col = 1; col <= rowData.Count; col++)
                    {
                        worksheet.Cells[targetRow, col].Value = rowData[col - 1];

                        // Nếu cần, có thể sao chép cả định dạng
                        worksheet.Cells[targetRow, col].StyleID = worksheet.Cells[row, col].StyleID;
                    }

                    var errorMessages = new List<string>();

                    foreach (var errorMessage in error.OrderBy(p => p.RowIndex))
                    {
                        var message = errorMessage.Message;
                        if (!string.IsNullOrEmpty(message))
                        {
                            errorMessages.Add(message);
                            int? num = columnIndexes?[errorMessage.ColumnName ?? string.Empty];
                            if (num.HasValue)
                            {
                                worksheet.Cells[targetRow, num.Value].Style.Fill.PatternType = ExcelFillStyle.Solid;
                                worksheet.Cells[targetRow, num.Value].Style.Fill.BackgroundColor.SetColor(Color.Red);
                                worksheet.Cells[targetRow, num.Value].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                                worksheet.Cells[targetRow, num.Value].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                                worksheet.Cells[targetRow, num.Value].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                                worksheet.Cells[targetRow, num.Value].Style.Border.Right.Style = ExcelBorderStyle.Thin;
                            }
                        }
                    }
                    var messages = Shared.Helpers.StringHelper.JoinWithComma(errorMessages.Distinct().ToList());
                    worksheet.Cells[targetRow, lastColumn].Value = messages;
                }
            };

            Action<ExcelWorksheet, Dictionary<string, int?>?, int, int, AddStudentsToCurriculumModel> defaultStudentHandlerAction = (worksheet, columnIndexes, row, num, model) =>
            {
                worksheet.Cells[row, num].Style.Fill.PatternType = ExcelFillStyle.Solid;
                worksheet.Cells[row, num].Style.Fill.BackgroundColor.SetColor(Color.White);
                worksheet.Cells[row, num].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                worksheet.Cells[row, num].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                worksheet.Cells[row, num].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                worksheet.Cells[row, num].Style.Border.Right.Style = ExcelBorderStyle.Thin;
                if (num == 6)
                {
                    worksheet.Cells[row, num + 1].Value = null;
                }
            };

            var cultureInfo = CultureInfo.InvariantCulture;

            var students = new List<AddStudentsToCurriculumModel>();

            var result = request.FormFile.ImportAndValidateExcel(async (AddStudentsToCurriculumModel x, IList<AddStudentsToCurriculumModel> models, int rowIndex, IList<ValidateExcelModel> errors) =>
            {
                if (string.IsNullOrEmpty(x.FullName?.Trim()))
                {
                    errors.Add(new ValidateExcelModel { RowIndex = rowIndex, ColumnName = nameof(x.FullName), Message = EmptyFullName });
                }
                if (string.IsNullOrEmpty(x.Email?.Trim()))
                {
                    errors.Add(new ValidateExcelModel { RowIndex = rowIndex, ColumnName = nameof(x.Email), Message = EmptyEmail });
                }
                else if (!x.Email.IsValidEmail())
                {
                    errors.Add(new ValidateExcelModel { RowIndex = rowIndex, ColumnName = nameof(x.Email), Message = InvalidEmail });
                }
                if (students.Contains(x))
                {
                    errors.Add(new ValidateExcelModel { RowIndex = rowIndex, ColumnName = nameof(x.Email), Message = DuplicateData });
                }
                else
                {
                    students.Add(new AddStudentsToCurriculumModel()
                    {
                        FullName = x.FullName,
                        Email = x.Email,
                    });
                }

                return await Task.FromResult(errors.Count == 0);
            },
            async (Dictionary<int, AddStudentsToCurriculumModel> datas, IList<ValidateExcelModel> errors) =>
            {
                var emails = datas.Values.Where(p => p.Email != null && !string.IsNullOrEmpty(p.Email.Trim())).Select(n => n.Email!.Trim());

                var emailsAlreadyExist = _userManager.Users.Where(x => emails.Contains(x.UserName)).Select(p => p.UserName);

                var emailsDoesNotExist = emails.Where(p => !emailsAlreadyExist.Contains(p));

                emailsDoesNotExist.ForEach(user =>
                {
                    var dataByEmail = datas.Values.Where(x => !x.Email.IsNullOrEmpty()).FirstOrDefault(x => (!string.IsNullOrEmpty(user) && x.Email.ToLower() == user));
                    if (dataByEmail == null)
                    {
                        var index = datas.FirstOrDefault(x => x.Value == dataByEmail).Key;
                        errors.Add(new ValidateExcelModel { RowIndex = index, ColumnName = nameof(dataByEmail.Email), Message = DataNotExist });
                    }
                });

                return await Task.FromResult(errors.Count == 0);
            },
             defaultStudentHandlerAction,
             errorHandlerAction,
             true);

            if (!result.IsValidHeader)
            {
                methodResult.AddErrorBadRequest(ErrorTemplate);
                return methodResult;
            }

            if (result.Stream != null)
            {
                methodResult.AddErrorBadRequest(DataError);
                return methodResult;
            }

            var emails = result.Datas.Where(p => p.Email != null && !string.IsNullOrEmpty(p.Email.Trim())).Select(n => n.Email!.Trim()).ToList();

            var query = await (from u in _userManager.Users.WhereBulkContains(emails, p => p.UserName)
                               join h in _humanRepository.Queryable on u.Id equals h.UserId
                               join s in _studentRepository.Queryable on h.Id equals s.HumanId
                               select s).ToListAsync(cancellationToken);

            var studentIds = query.Select(p => p.Id).ToList();

            var addStudentsResult = await _lmsCourseService.AddStudentToCurriculum(new AddStudentsToCurriculumCommandModel()
            {
                CurriculumId = request.CurriculumId,
                StudentIds = studentIds
            });

            if (!addStudentsResult.IsSuccessStatusCode)
            {
                methodResult.AddError(addStudentsResult.Error);
                return methodResult;
            }

            return methodResult;
        }
    }
}
