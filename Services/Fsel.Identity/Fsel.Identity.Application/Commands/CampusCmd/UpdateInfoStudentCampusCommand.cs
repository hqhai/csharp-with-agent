// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Commands.CampusCmd
{
    using System.Drawing;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Common.Helpers;
    using Fsel.Common.Models.Excels;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Core.Base.Managers;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Identity.Domain.IRepositories;
    using Fsel.Identity.Domain.Models.CommandModels.Campus;
    using Fsel.Identity.Infrastructure.Repositories;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;
    using OfficeOpenXml;
    using OfficeOpenXml.Style;

    public class UpdateInfoStudentCampusCommand : BaseImportCommandModel, IRequest<MethodResult<Stream>>
    {
    }

    public class UpdateInfoStudentCampusCommandHandler : IRequestHandler<UpdateInfoStudentCampusCommand, MethodResult<Stream>>
    {
        private readonly UserManager<User> _userManager;
        private readonly IStudentRepository _studentRepository;
        private readonly RoleManager<Role> _roleManager;
        private readonly IUserRoleRepository _userRoleRepository;

        private const string ErrorMessage = "Thông báo lỗi";
        private const string ErrorTemplate = "Template bị sai, kiểm tra lại tên cột";

        public UpdateInfoStudentCampusCommandHandler(UserManager<User> userManager, IStudentRepository studentRepository, RoleManager<Role> roleManager, IUserRoleRepository userRoleRepository)
        {
            _userManager = userManager;
            _studentRepository = studentRepository;
            _roleManager = roleManager;
            _userRoleRepository = userRoleRepository;
        }

        public async Task<MethodResult<Stream>> Handle(UpdateInfoStudentCampusCommand request, CancellationToken cancellationToken)
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
                worksheet.Cells[1, 4].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                worksheet.Cells[1, 4].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                worksheet.Cells[1, 4].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                worksheet.Cells[1, 4].Style.Border.Right.Style = ExcelBorderStyle.Thin;
                worksheet.Cells[1, 4].Value = ErrorMessage;
                worksheet.Cells[1, 4].Style.Font.Bold = true;
                foreach (var error in errors.GroupBy(x => x.RowIndex).Select(x => x).OrderBy(x => x.Key))
                {
                    var row = error.Key;

                    int lastColumn = 4;

                    List<object?> rowData = new List<object?>();

                    for (int col = 1; col <= worksheet.Dimension.End.Column; col++)
                    {
                        rowData.Add(worksheet.Cells[row, col].Value);
                    }

                    int targetRow = 2;
                    worksheet.DeleteRow(row, 1);
                    worksheet.InsertRow(targetRow, 1);

                    for (int col = 1; col <= rowData.Count; col++)
                    {
                        worksheet.Cells[targetRow, col].Value = rowData[col - 1];
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

            Action<ExcelWorksheet, Dictionary<string, int?>?, int, int, UpdateInfoStudentCampusModel> defaultStudentHandlerAction = (worksheet, columnIndexes, row, num, model) =>
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

            var students = new List<UpdateInfoStudentCampusModel>();

            var result = request.FormFile.ImportAndValidateExcel(async (UpdateInfoStudentCampusModel x, IList<UpdateInfoStudentCampusModel> models, int rowIndex, IList<ValidateExcelModel> errors) =>
            {
                if (!string.IsNullOrEmpty(x.Email?.Trim()))
                {
                    if (!x.Email.IsValidEmail())
                    {
                        errors.Add(new ValidateExcelModel { RowIndex = rowIndex, ColumnName = nameof(x.Email), Message = ErrorMassageSetting.InvalidEmailVN });
                    }
                    students.Add(new UpdateInfoStudentCampusModel()
                    {
                        Email = x.Email.Trim(),
                        SchoolClass = !string.IsNullOrEmpty(x.SchoolClass?.Trim()) ? x.SchoolClass?.Trim() : null,
                        StudentCode = !string.IsNullOrEmpty(x.StudentCode?.Trim()) ? x.StudentCode?.Trim() : null,
                    });
                }

                return await Task.FromResult(errors.Count == 0);
            },
            null,
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
                methodResult.Result = result.Stream;
                methodResult.StatusCode = StatusCodes.Status400BadRequest;
                return methodResult;
            }

            students = students.DistinctBy(p => p.Email).ToList();

            if (!students.Any())
            {
                methodResult.AddErrorBadRequest(ErrorMassageSetting.FileNull);
                return methodResult;
            }

            var query = await (from u in _userManager.Users

                               join ur in _userRoleRepository.GetQuery() on u.Id equals ur.UserId

                               join r in _roleManager.Roles on ur.RoleId equals r.Id

                               join s in _studentRepository.Queryable on u.Id equals s.UserId

                               where r.Name == EnumRole.StudentCampus.ToString()

                               select new { User = u, Student = s }).ToListAsync(cancellationToken);

            var studentUpdates = new List<Student>();

            students.ForEach(student =>
            {
                var user = query.FirstOrDefault(x => x.User.Email == student.Email);
                if (user != null)
                {
                    user.Student.StudentCampusCode = student.StudentCode;
                    user.Student.ClassCampusCode = student.SchoolClass;
                    studentUpdates.Add(user.Student);
                }
            });

            if (studentUpdates.Count > 0)
            {
                await _studentRepository.ExecuteTransactionAsync(async () =>
                {
                    await _studentRepository.BulkMergeAsync(studentUpdates);
                    return methodResult;
                });
            }
            return methodResult;
        }
    }
}
