// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Commands.AdminCmd
{
    using System.Globalization;
    using System.Linq;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Common.Helpers;
    using Fsel.Common.Models.Excels;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Identity.Domain.Enums.ErrorCodes;
    using Fsel.Identity.Domain.Models.CommandModels.Admins;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Helpers;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.EntityFrameworkCore;

    public class ImportFileAdminSchoolsCommand : BaseImportCommandModel, IRequest<MethodResult<Stream>>
    {
    }

    public class ImportFileAdminSchoolsCommandHandler : IRequestHandler<ImportFileAdminSchoolsCommand, MethodResult<Stream>>
    {
        private readonly Core.Base.Managers.UserManager<User> _userManager;
        private readonly Microsoft.AspNetCore.Identity.RoleManager<Role> _roleManager;
        private const string DefaultPassword = "Admin@123";

        public ImportFileAdminSchoolsCommandHandler(Core.Base.Managers.UserManager<User> userManager, Microsoft.AspNetCore.Identity.RoleManager<Role> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<MethodResult<Stream>> Handle(ImportFileAdminSchoolsCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<Stream>();

            if (request.FormFile == null)
            {
                methodResult.AddError(nameof(EnumSystemErrorCode.ImportFileRequired));
                return methodResult;
            }
            var result = request.FormFile.ImportAndValidateExcel(async (CreateAdminSchoolCommandModel x, IList<CreateAdminSchoolCommandModel> models, int rowIndex, IList<ValidateExcelModel> errors) =>
            {
                if (string.IsNullOrEmpty(x.FullName))
                {
                    errors.Add(new ValidateExcelModel { RowIndex = rowIndex, ColumnName = nameof(x.FullName), Message = "Full Name is null" });
                }
                if (string.IsNullOrEmpty(x.SchoolId))
                {
                    errors.Add(new ValidateExcelModel { RowIndex = rowIndex, ColumnName = nameof(x.SchoolId), Message = $"{nameof(x.SchoolId)} is null" });
                }
                else if (!Guid.TryParse(x.SchoolId, out Guid schoolId))
                {
                    errors.Add(new ValidateExcelModel { RowIndex = rowIndex, ColumnName = nameof(x.SchoolId), Message = $"{nameof(x.SchoolId)} {nameof(EnumSystemErrorCode.InValidFormat)}" });
                }
                if (string.IsNullOrEmpty(x.Email) || !x.Email.IsValidEmail())
                {
                    errors.Add(new ValidateExcelModel { RowIndex = rowIndex, ColumnName = nameof(x.Email), Message = "Email is null or malformed" });
                }
                if (!string.IsNullOrEmpty(x.Password))
                {
                    foreach (IPasswordValidator<User> passwordValidator in _userManager.PasswordValidators)
                    {
                        var resultData = await passwordValidator.ValidateAsync(_userManager, new User(), x.Password);
                        if (!resultData.Succeeded)
                        {
                            errors.Add(new ValidateExcelModel { RowIndex = rowIndex, ColumnName = nameof(x.Password), Message = $"Password is {EnumSystemErrorCode.InValidFormat}" });
                        }
                    }
                }
                return await Task.FromResult(errors.Count == 0);
            });
            if (result.Stream != null)
            {
                methodResult.Result = result.Stream;
                methodResult.StatusCode = StatusCodes.Status400BadRequest;
                return methodResult;
            }
            var emails = result.Datas.Where(x => !string.IsNullOrEmpty(x.Email)).Select(x => x.Email!.Trim()).ToList();
            var emailsDuplicates = emails.GroupBy(x => x).Where(x => x.Count() > 1).Select(x => x.Key).ToList();

            if (emailsDuplicates.Any())
            {
                methodResult.AddErrorBadRequest(new List<ErrorResult>
                {
                    new ErrorResult
                    {
                        ErrorCode = nameof(emailsDuplicates),
                        Errors = new List<Error>
                        {
                            new Error
                            {
                                FieldName  = nameof(emailsDuplicates),
                                ErrorValues = emailsDuplicates.Cast<object>().ToList()
                            }
                        }
                    }
                });
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
            var users = await _userManager.Users.Where(x => !string.IsNullOrEmpty(x.Email) && emails.Contains(x.Email)).ToListAsync(cancellationToken);
            foreach (var item in result.Datas)
            {
                if (string.IsNullOrEmpty(item.Email))
                {
                    continue;
                }
                if (users.Any(x => !string.IsNullOrEmpty(x.Email) && x.Email.Contains(item.Email, StringComparison.CurrentCulture)))
                {
                    continue;
                }
                try
                {
                    var user = new User();
                    user.UserName = item.Email;
                    user.Email = item.Email;
                    user.FirstName = item.FullName.ParseFullName().FirstName;
                    user.LastName = item.FullName.ParseFullName().LastName;
                    user.EmailConfirmed = true;
                    if (Guid.TryParse(item.SchoolId, CultureInfo.InvariantCulture, out Guid schoolId))
                    {
                        user.UserSchools = new List<UserSchool>
                        {
                            new UserSchool { SchoolId = schoolId }
                        };
                    }

                    if (!user.IsValid())
                    {
                        methodResult.AddErrorBadRequest(user.ErrorMessages);
                        return methodResult;
                    }
                    var resultUser = await _userManager.CreateAsync(user, item.Password ?? DefaultPassword);
                    if (!resultUser.Succeeded)
                    {
                        methodResult.AddErrorBadRequest(nameof(EnumAuthUserErrorCode.UserFailToCreate));
                        return methodResult;
                    }
                    await _userManager.AddToRoleAsync(user, nameof(EnumRole.AdminSchool));
                }
                catch (Exception e)
                {
                    methodResult.AddError(nameof(EnumAuthUserErrorCode.SendAuthErorr), e.Message);
                    return methodResult;
                }
            }
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
