// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Commands.CampusCmd
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
    using Fsel.Identity.Application.Services.LmsCourseService.QueryModels;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Identity.Domain.IRepositories;
    using Fsel.Identity.Domain.Models;
    using Fsel.Identity.Domain.Models.CommandModels.Campus;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Models.ShareModels.CampusModel;
    using Kros.Extensions;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;
    using OfficeOpenXml;
    using OfficeOpenXml.Style;

    public class AddStudentsToCurriculumActiveCommand : BaseImportCommandModel, IRequest<MethodResult<AddStudentIntoSchoolClassCommandModel>>
    {
        public Guid CurriculumId { get; set; }
    }

    public class AddStudentsToCurriculumActiveCommandHandler : IRequestHandler<AddStudentsToCurriculumActiveCommand, MethodResult<AddStudentIntoSchoolClassCommandModel>>
    {
        private readonly IStudentRepository _studentRepository;
        private readonly UserManager<User> _userManager;
        private readonly ILmsCourseService _lmsCourseService;

        private const string ErrorMessage = "Thông báo lỗi";
        private const string EmptyFullName = "Chưa nhập Họ và Tên";
        private const string EmptyUserName = "Chưa nhập tên tài khoản";
        private const string DuplicateData = "Tên tài khoản trùng lặp trong file tải lên";
        private const string DataNotExist = "Tên tài khoản không tồn tại trên hệ thống";
        private const string MismatchedData = "Dữ liệu không trùng khớp";
        private const string ErrorTemplate = "Template bị sai, kiểm tra lại tên cột, bạn cần download template ở nút Tải Template mẫu";
        private const string DataAlreadyExist = "Học sinh đã được thêm vào giáo trình này rồi";

        public AddStudentsToCurriculumActiveCommandHandler(IStudentRepository studentRepository, UserManager<User> userManager, ILmsCourseService lmsCourseService)
        {
            _studentRepository = studentRepository;
            _userManager = userManager;
            _lmsCourseService = lmsCourseService;
        }

        public async Task<MethodResult<AddStudentIntoSchoolClassCommandModel>> Handle(AddStudentsToCurriculumActiveCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<AddStudentIntoSchoolClassCommandModel>();

            if (request.FormFile == null)
            {
                methodResult.AddError(nameof(EnumSystemErrorCode.ImportFileRequired));
                return methodResult;
            }

            var curriculumResult = await _lmsCourseService.GetCurriculumById(new GetCurriculumByIdQueryModel() { Id = request.CurriculumId });
            var curriculum = curriculumResult.Content?.Result;
            if (curriculum == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(curriculum));
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

            var studentIdsResult = await _lmsCourseService.GetStudentIdsByCurriculumId(request.CurriculumId);
            var curriculumStudentIds = studentIdsResult.Content?.Result;

            var result = request.FormFile.ImportAndValidateExcel(async (AddStudentsToCurriculumModel x, IList<AddStudentsToCurriculumModel> models, int rowIndex, IList<ValidateExcelModel> errors) =>
            {
                if (!string.IsNullOrEmpty(x.Username?.Trim()) || !string.IsNullOrEmpty(x.FullName?.Trim()))
                {
                    if (string.IsNullOrEmpty(x.FullName?.Trim()))
                    {
                        errors.Add(new ValidateExcelModel { RowIndex = rowIndex, ColumnName = nameof(x.FullName), Message = EmptyFullName });
                    }
                    if (string.IsNullOrEmpty(x.Username?.Trim()))
                    {
                        errors.Add(new ValidateExcelModel { RowIndex = rowIndex, ColumnName = nameof(x.Username), Message = EmptyUserName });
                    }
                    if (students.Select(p => p.Username).Contains(x.Username))
                    {
                        errors.Add(new ValidateExcelModel { RowIndex = rowIndex, ColumnName = nameof(x.Username), Message = DuplicateData });
                    }
                    else
                    {
                        students.Add(new AddStudentsToCurriculumModel()
                        {
                            FullName = x.FullName,
                            Username = x.Username,
                        });
                    }
                }

                return await Task.FromResult(errors.Count == 0);
            },
            async (Dictionary<int, AddStudentsToCurriculumModel> datas, IList<ValidateExcelModel> errors) =>
            {
                var emails = datas.Values.Where(p => p.Username != null && !string.IsNullOrEmpty(p.Username.Trim())).Select(n => n.Username?.Trim() ?? string.Empty);

                var query = await (from u in _userManager.Users.WhereBulkContains(emails, p => p.UserName)
                                   join s in _studentRepository.Queryable on u.Id equals s.UserId
                                   select new
                                   {
                                       User = u,
                                       Student = s
                                   }).ToListAsync(cancellationToken);

                var users = query.Select(p => p.User).ToList();

                var usernamesAlreadyExist = users.Select(p => p.UserName).ToList();

                var usernamesDoesNotExist = emails.Where(p => !usernamesAlreadyExist.Contains(p)).ToList();

                usernamesDoesNotExist.ForEach(user =>
                {
                    var dataByEmail = datas.Values.Where(x => !x.Username.IsNullOrEmpty()).FirstOrDefault(x => (!string.IsNullOrEmpty(user) && x.Username?.ToLower(CultureInfo.CurrentCulture) == user.ToLower(CultureInfo.CurrentCulture)));
                    if (dataByEmail != null)
                    {
                        var index = datas.FirstOrDefault(x => x.Value == dataByEmail).Key;
                        errors.Add(new ValidateExcelModel { RowIndex = index, ColumnName = nameof(dataByEmail.Username), Message = DataNotExist });
                    }
                });

                query.ForEach(user =>
                {
                    var dataByEmail = datas.Values.Where(x => !x.Username.IsNullOrEmpty()).FirstOrDefault(x => (!string.IsNullOrEmpty(user.User.UserName) && x.Username?.ToLower(CultureInfo.CurrentCulture) == user.User.UserName.ToLower(CultureInfo.CurrentCulture)));
                    if (dataByEmail != null && dataByEmail.FullName?.ToLower(CultureInfo.CurrentCulture) != user.User.FullName?.ToLower(CultureInfo.CurrentCulture))
                    {
                        var index = datas.FirstOrDefault(x => x.Value == dataByEmail).Key;
                        errors.Add(new ValidateExcelModel { RowIndex = index, ColumnName = nameof(dataByEmail.Username), Message = MismatchedData });
                    }
                    else if (curriculumStudentIds != null && curriculumStudentIds.Any() && curriculumStudentIds.Contains(user.Student.Id))
                    {
                        var index = datas.FirstOrDefault(x => x.Value == dataByEmail).Key;
                        errors.Add(new ValidateExcelModel { RowIndex = index, ColumnName = nameof(dataByEmail.Username), Message = DataAlreadyExist });
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
                methodResult.Result = new AddStudentIntoSchoolClassCommandModel() { Stream = result.Stream };
                methodResult.StatusCode = StatusCodes.Status400BadRequest;
                return methodResult;
            }

            if (!students.Any())
            {
                methodResult.AddErrorBadRequest(ErrorMassageSetting.FileNull);
                return methodResult;
            }

            var usernames = result.Datas.Where(p => p.Username != null && !string.IsNullOrEmpty(p.Username.Trim())).Select(n => n.Username!.Trim()).ToList();

            var query = await (from u in _userManager.Users.WhereBulkContains(usernames, p => p.UserName)
                               join s in _studentRepository.Queryable on u.Id equals s.UserId
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
            methodResult.Result = new AddStudentIntoSchoolClassCommandModel() { NumberOfStudent = studentIds.Count };
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
