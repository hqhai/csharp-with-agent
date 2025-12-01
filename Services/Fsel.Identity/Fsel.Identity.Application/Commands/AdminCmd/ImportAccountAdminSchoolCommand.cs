// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Commands.AdminCmd
{
    using System.Drawing;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Common.Helpers;
    using Fsel.Common.Models.Excels;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Core.Base.Managers;
    using Fsel.Identity.Application.Services.SystemService;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Identity.Domain.IRepositories;
    using Fsel.Identity.Domain.Models.CommandModels.Admins;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Helpers;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.DependencyInjection;
    using OfficeOpenXml;
    using OfficeOpenXml.Style;

    public class ImportAccountAdminSchoolCommand : BaseImportCommandModel, IRequest<MethodResult<ImportAccountAdminSchoolModel>>
    {
    }

    public class ImportAccountAdminSchoolCommandHandler : IRequestHandler<ImportAccountAdminSchoolCommand, MethodResult<ImportAccountAdminSchoolModel>>
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<Role> _roleManager;
        private readonly ISystemService _systemService;
        private readonly IUserSchoolRepository _userSchoolRepository;
        private readonly ICompetitionEventsRepository _competitionEventsRepository;
        private readonly IServiceProvider _serviceProvider;
        private const string ErrorMessage = "Error Message\n(Thông báo lỗi)";
        private const string SchoolName = "Tên trường không đúng định dạng";
        private const string LocalIdNotExist = "Không có thông tin trường học này";
        private const string UserNameInValid = "Username không đúng định dạng";
        private const string UserNameDuplicate = "Username này đã tồn tại trên hệ thống";
        private const string UserNameExistInList = "Username bị trùng trong danh sách";
        private const string PasswordInValid = "Mật khẩu không đúng định dạng";
        private const string EmailNull = "Email không được bỏ trống";
        private const string EmailInValid = " Email không đúng định dạng";
        private const string EmailDuplicate = "Email này đã tồn tại trên hệ thống";
        private const string EmailAlreadyExistInFile = "Email đã tồn tại trong file";
        private const string DefaultPasswork = "Fsel@2025";


        public ImportAccountAdminSchoolCommandHandler(UserManager<User> userManager,
                                                      RoleManager<Role> roleManager,
                                                      ISystemService systemService,
                                                      IUserSchoolRepository userSchoolRepository,
                                                      ICompetitionEventsRepository competitionEventsRepository,
                                                      IServiceProvider serviceProvider)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _systemService = systemService;
            _userSchoolRepository = userSchoolRepository;
            _competitionEventsRepository = competitionEventsRepository;
            _serviceProvider = serviceProvider;
        }

        public async Task<MethodResult<ImportAccountAdminSchoolModel>> Handle(ImportAccountAdminSchoolCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<ImportAccountAdminSchoolModel> methodResult = new MethodResult<ImportAccountAdminSchoolModel>();
            //var regexSchoolName = new Regex("^[a-zA-Z0-9]+$");
            var regexPassword = new Regex("^(?=.*[a-z])(?=.*[A-Z])(?=.*\\d)(?=.*[^a-zA-Z0-9]).{8,}$");
            if (request.FormFile == null)
            {
                methodResult.AddError(nameof(EnumSystemErrorCode.ImportFileRequired));
                return methodResult;
            }

            Action<ExcelWorksheet, Dictionary<string, int?>?, IList<ValidateExcelModel>> errorHandlerAction = (worksheet, columnIndexes, errors) =>
            {
                worksheet.Cells[1, 6].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                worksheet.Cells[1, 6].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                worksheet.Cells[1, 6].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                worksheet.Cells[1, 6].Style.Border.Right.Style = ExcelBorderStyle.Thin;
                worksheet.Cells[1, 6].Value = ErrorMessage;
                worksheet.Cells[1, 6].Style.Font.Bold = true;
                foreach (var error in errors.GroupBy(x => x.RowIndex).Select(x => x).OrderBy(x => x.Key))
                {
                    var errorMessages = new List<string>();
                    int targetRow = error.Key;

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
                    worksheet.Cells[targetRow, 6].Value = messages;
                }
            };

            var datas = new List<ImportAccountAdminSchoolCommandModel>();

            var result = request.FormFile.ImportAndValidateExcel(async (ImportAccountAdminSchoolCommandModel x, IList<ImportAccountAdminSchoolCommandModel> models, int rowIndex, IList<ValidateExcelModel> errors) =>
            {
                if (string.IsNullOrEmpty(x.SchoolName))
                {
                    errors.Add(new ValidateExcelModel { RowIndex = rowIndex, ColumnName = nameof(x.SchoolName), Message = SchoolName });
                }

                var checkLocalId = await _systemService.GetLocationByLocalId(x.LocalId ?? string.Empty);
                if (string.IsNullOrEmpty(x.LocalId) || checkLocalId.Content?.Result == null)
                {
                    errors.Add(new ValidateExcelModel { RowIndex = rowIndex, ColumnName = nameof(x.LocalId), Message = LocalIdNotExist });
                }

                if (string.IsNullOrEmpty(x.UserName) || x.UserName.Length < 6 || x.UserName.Contains(" "))
                {
                    errors.Add(new ValidateExcelModel { RowIndex = rowIndex, ColumnName = nameof(x.UserName), Message = UserNameInValid });
                }

                if (!string.IsNullOrEmpty(x.UserName) && await _userManager.Users.AnyAsync(c => c.UserName == x.UserName.Trim(), cancellationToken))
                {
                    errors.Add(new ValidateExcelModel { RowIndex = rowIndex, ColumnName = nameof(x.UserName), Message = UserNameDuplicate });
                }

                if (datas.Any(c => c.UserName == x.UserName))
                {
                    errors.Add(new ValidateExcelModel { RowIndex = rowIndex, ColumnName = nameof(x.UserName), Message = UserNameExistInList });
                }

                if (string.IsNullOrEmpty(x.Password) || !regexPassword.IsMatch(x.Password))
                {
                    errors.Add(new ValidateExcelModel { RowIndex = rowIndex, ColumnName = nameof(x.Password), Message = PasswordInValid });
                }

                if (string.IsNullOrEmpty(x.Email))
                {
                    errors.Add(new ValidateExcelModel { RowIndex = rowIndex, ColumnName = nameof(x.Email), Message = EmailNull });
                }

                if (!x.Email.IsValidEmail())
                {
                    errors.Add(new ValidateExcelModel { RowIndex = rowIndex, ColumnName = nameof(x.Email), Message = EmailInValid });
                }

                if (!string.IsNullOrEmpty(x.Email) && await _userManager.Users.AnyAsync(c => c.Email == x.Email.Trim()))
                {
                    errors.Add(new ValidateExcelModel { RowIndex = rowIndex, ColumnName = nameof(x.Email), Message = EmailDuplicate });
                }

                if (!string.IsNullOrEmpty(x.Email) && datas.Any(m => m.Email == x.Email))
                {
                    errors.Add(new ValidateExcelModel { RowIndex = rowIndex, ColumnName = nameof(x.Email), Message = EmailAlreadyExistInFile });
                }


                datas.Add(x);

                return await Task.FromResult(errors.Count == 0);

            }, null, null, errorHandlerAction, true);

            if (result.Stream != null)
            {
                methodResult.Result = new ImportAccountAdminSchoolModel { Stream = result.Stream };
                methodResult.StatusCode = StatusCodes.Status400BadRequest;
                return methodResult;
            }

            if (!result.Datas.Any())
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                return methodResult;
            }


            var role = await _roleManager.FindByNameAsync(nameof(EnumRole.AdminSchool));
            if (role == null)
            {
                role = new Role
                {
                    Name = nameof(EnumRole.AdminSchool),
                    NormalizedName = nameof(EnumRole.AdminSchool),
                };
                await _roleManager.CreateAsync(role);
            }

            int countAccount = 0;

            var parallelOptions = new ParallelOptions
            {
                MaxDegreeOfParallelism = 50
            };

            await Parallel.ForEachAsync(result.Datas, parallelOptions, async (item, cancellationToken) =>
            {
                try
                {
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
                        var competitionEventsRepository = scope.ServiceProvider.GetRequiredService<ICompetitionEventsRepository>();
                        var userSchoolRepository = scope.ServiceProvider.GetRequiredService<IUserSchoolRepository>();

                        Microsoft.AspNetCore.Identity.IdentityResult identityStudentResult;
                        var user = new User();
                        user.UserName = item.UserName;
                        user.Email = item.Email;
                        user.FirstName = item.UserName.ParseFullName().FirstName;
                        user.LastName = item.UserName.ParseFullName().LastName;
                        user.EmailConfirmed = true;
                        user.DefaultPassword = item.Password ?? DefaultPasswork;
                        identityStudentResult = await userManager.CreateAsync(user, item.Password ?? DefaultPasswork);
                        if (identityStudentResult.Succeeded)
                        {
                            await userManager.AddToRoleAsync(user, EnumRole.AdminSchool.ToString());
                            var schoolQuery = await _systemService.GetLocationByLocalId(item.LocalId ?? string.Empty);
                            var school = schoolQuery.Content?.Result;
                            if (school != null)
                            {
                                var eventCode = await competitionEventsRepository.Queryable.FirstOrDefaultAsync(x => x.LocationId == (school.ParentId ?? Guid.Empty) && x.Category == EnumCompetitionEventCategory.Student, cancellationToken);
                                var city = school.LongPath?.Split('/')[2];
                                userSchoolRepository.Add(new UserSchool
                                {
                                    UserId = user.Id,
                                    SchoolId = school.Id,
                                    SchoolName = school.Name,
                                    LocalId = item.LocalId,
                                    City = city,
                                    EventCode = eventCode?.EventCode
                                });
                                await userSchoolRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                            }

                            Interlocked.Increment(ref countAccount);
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception();
                }
            });

            methodResult.Result = new ImportAccountAdminSchoolModel { CountAccount = countAccount };
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
