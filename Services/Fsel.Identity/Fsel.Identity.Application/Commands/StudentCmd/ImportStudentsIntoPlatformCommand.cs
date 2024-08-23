// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Commands.StudentCmd
{
    using System.Globalization;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Common.Helpers;
    using Fsel.Common.Models.Excels;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Core.Base.Managers;
    using Fsel.Identity.Application.Commands.UserCmd;
    using Fsel.Identity.Application.Services.InteractionService;
    using Fsel.Identity.Application.Services.OrderService;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Identity.Domain.Enums;
    using Fsel.Identity.Domain.IRepositories;
    using Fsel.Identity.Domain.Models.CommandModels.Students;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;
    using EnumAuthUserErrorCode = Domain.Enums.ErrorCodes.EnumAuthUserErrorCode;

    public class ImportStudentsIntoPlatformCommand : BaseImportCommandModel, IRequest<MethodResult<Stream>>
    {
    }

    public class ImportStudentsIntoPlatformCommandHandler : IRequestHandler<ImportStudentsIntoPlatformCommand, MethodResult<Stream>>
    {
        private readonly UserManager<User> _userManager;
        private readonly IOrderService _orderService;
        private readonly IPlatformRepository _platformRepository;
        private const string DefaultPassword = "Fsel@2024";
        private readonly IInteractionService _interactionService;
        private readonly IMediator _mediator;
        private readonly IHumanRepository _humanRepository;

        public ImportStudentsIntoPlatformCommandHandler(UserManager<User> userManager, IOrderService orderService, IPlatformRepository platformRepository, IInteractionService interactionService, IMediator mediator, IHumanRepository humanRepository)
        {
            _userManager = userManager;
            _orderService = orderService;
            _platformRepository = platformRepository;
            _interactionService = interactionService;
            _mediator = mediator;
            _humanRepository = humanRepository;
        }

        public async Task<MethodResult<Stream>> Handle(ImportStudentsIntoPlatformCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<Stream>();

            if (request.FormFile == null)
            {
                methodResult.AddError(nameof(EnumSystemErrorCode.ImportFileRequired));
                return methodResult;
            }

            var packagesResult = await _orderService.GetPackages();

            var result = request.FormFile.ImportAndValidateExcel(async (ImportStudentToPlatformModel x, IList<ImportStudentToPlatformModel> models, int rowIndex, IList<ValidateExcelModel> errors) =>
            {
                if (string.IsNullOrEmpty(x.FullName))
                {
                    errors.Add(new ValidateExcelModel { RowIndex = rowIndex, ColumnName = nameof(x.FullName), Message = "Full Name is null" });
                }
                if (string.IsNullOrEmpty(x.Email) || !x.Email.IsValidEmail())
                {
                    errors.Add(new ValidateExcelModel { RowIndex = rowIndex, ColumnName = nameof(x.Email), Message = "Email is null or malformed" });
                }
                else if (_userManager.Users.Any(p => p.Email == x.Email || p.UserName == x.Email))
                {
                    errors.Add(new ValidateExcelModel { RowIndex = rowIndex, ColumnName = nameof(x.Email), Message = "Email Already exist" });
                }
                if (!string.IsNullOrEmpty(x.ParentEmail) && !x.ParentEmail.IsValidEmail())
                {
                    errors.Add(new ValidateExcelModel { RowIndex = rowIndex, ColumnName = nameof(x.ParentEmail), Message = "Parent Email is null or malformed" });
                }
                if (string.IsNullOrEmpty(x.DateOfBirth) || (!string.IsNullOrEmpty(x.DateOfBirth) && !DateTime.TryParse(x.DateOfBirth, out DateTime dob)))
                {
                    errors.Add(new ValidateExcelModel { RowIndex = rowIndex, ColumnName = nameof(x.DateOfBirth), Message = "Date of birth is null or malformed" });
                }
                if (!string.IsNullOrEmpty(x.ReferralCode) && !await _humanRepository.Queryable.AnyAsync(p => p.Code == x.ReferralCode))
                {
                    errors.Add(new ValidateExcelModel { RowIndex = rowIndex, ColumnName = nameof(x.ReferralCode), Message = "Referral code not exist" });
                }
                return await Task.FromResult(errors.Count == 0);
            });

            var duplicateEmails = result.Datas.GroupBy(user => user.Email).Where(group => group.Count() > 1).Select(group => group.Key);

            if (duplicateEmails.Any())
            {
                methodResult.AddErrorBadRequest("Duplicate Emails");
                return methodResult;
            }

            if (result.Stream != null)
            {
                methodResult.Result = result.Stream;
                methodResult.StatusCode = StatusCodes.Status400BadRequest;
                return methodResult;
            }

            var platform = await _platformRepository.GetPlatformAsync(EnumPlatformCode.LMS, cancellationToken);
            if (platform == null)
            {
                methodResult.AddErrorBadRequest("Platform null");
                return methodResult;
            }
            var users = new List<User>();

            try
            {
                foreach (var student in result.Datas.ToList())
                {
                    Microsoft.AspNetCore.Identity.IdentityResult identityStudentResult;
                    Microsoft.AspNetCore.Identity.IdentityResult identityParentResult;
                    var user = new User()
                    {
                        UserName = student.Email,
                        Email = student.Email,
                        FullName = student.FullName,
                        EmailConfirmed = true,
                        Human = new Human()
                        {
                            FullName = student.FullName,
                            Birthday = Convert.ToDateTime(student.DateOfBirth, CultureInfo.CurrentCulture),
                            Email = student.Email,
                            Student = new Student()
                            {
                                CreatedByParent = false,
                                Occupation = "Student"
                            }
                        },
                        UserPlatforms = new List<UserPlatform>()
                                            {
                                                new UserPlatform()
                                                {
                                                    PlatformId = platform.Id
                                                }
                                            },
                        UserSettings = new List<UserSetting>()
                            {
                                new UserSetting(true)
                            }
                    };

                    identityStudentResult = await _userManager.CreateAsync(user, DefaultPassword);
                    if (!identityStudentResult.Succeeded)
                    {
                        methodResult.AddErrorBadRequest(nameof(EnumAuthUserErrorCode.UserFailToCreate));
                        return methodResult;
                    }
                    await _userManager.AddToRoleAsync(user, EnumRole.Student.ToString());
                    if (!string.IsNullOrEmpty(student.ParentEmail))
                    {
                        var parent = await _userManager.Users.Include(p => p.Human).ThenInclude(p => p.Parent).FirstOrDefaultAsync(p => p.UserName == student.ParentEmail || p.Email == student.ParentEmail, cancellationToken);
                        if (parent == null)
                        {
                            parent = new User()
                            {
                                UserName = student.ParentEmail,
                                Email = student.ParentEmail,
                                FullName = student.ParentEmail,
                                EmailConfirmed = true,
                                Human = new Human()
                                {
                                    FullName = student.ParentEmail,
                                    Email = student.ParentEmail,
                                    Parent = new Parent()
                                    {
                                        Occupation = "Parent",
                                    }
                                },
                                UserPlatforms = new List<UserPlatform>()
                                {
                                    new UserPlatform()
                                    {
                                        PlatformId = platform.Id
                                    }
                                }
                            };

                            identityParentResult = await _userManager.CreateAsync(parent, DefaultPassword);
                            if (!identityParentResult.Succeeded)
                            {
                                methodResult.AddErrorBadRequest(nameof(EnumAuthUserErrorCode.UserFailToCreate));
                                return methodResult;
                            }
                            await _userManager.AddToRoleAsync(parent, EnumRole.Parent.ToString());
                        }
                        if (parent.Human?.Parent != null)
                        {
                            user.Human.Student.ParentStudents.Add(new ParentStudent
                            {
                                ParentId = parent.Human.Parent.Id,
                            });
                            await _userManager.UpdateAsync(user);
                        }
                    }

                    var updateCode = await _mediator.Send(new UpdateCodeStudentCommand { UserId = user.Id, Gender = EnumGender.Male, Birthday = user.Human.Birthday, SchoolName = student.School }, cancellationToken);
                    if (!updateCode.IsOK)
                    {
                        methodResult.AddErrorBadRequest(updateCode.ErrorMessages);
                        return methodResult;
                    }

                    if (!string.IsNullOrEmpty(student.ReferralCode))
                    {
                        var updateReferralCodeResult = await _mediator.Send(new UpdateReferralCodeStudentCommand { ReferralCode = student.ReferralCode, UserId = user.Id }, cancellationToken).ConfigureAwait(false);
                        if (!updateReferralCodeResult.IsOK)
                        {
                            methodResult.AddErrorBadRequest(updateReferralCodeResult.ErrorMessages);
                            return methodResult;
                        }
                    }
                }
                methodResult.StatusCode = StatusCodes.Status200OK;
                return methodResult;
            }
            catch (Exception ex)
            {
                methodResult.AddErrorBadRequest(ex.Message);
            }

            return methodResult;
        }
    }
}
