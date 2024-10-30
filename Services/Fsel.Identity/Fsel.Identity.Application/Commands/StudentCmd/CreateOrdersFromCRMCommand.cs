// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Commands.StudentCmd
{
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Common.Helpers;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Core.Base.Managers;
    using Fsel.Identity.Application.Commands.AuthCmd;
    using Fsel.Identity.Application.Services.InteractionService;
    using Fsel.Identity.Application.Services.InteractionService.Models;
    using Fsel.Identity.Application.Services.OrderService;
    using Fsel.Identity.Application.Services.OrderService.Model;
    using Fsel.Identity.Application.Services.SystemService;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Identity.Domain.Enums.ErrorCodes;
    using Fsel.Identity.Domain.IRepositories;
    using Fsel.Identity.Domain.Models.CommandModels.Students;
    using Fsel.Identity.Infrastructure.ValueSettings;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Helpers;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class CreateOrdersFromCRMCommand : CreateOrdersFromCRMCommandModels, IRequest<VoidMethodResult>
    {
    }

    public class CreateOrdersFromCRMCommandHandler : IRequestHandler<CreateOrdersFromCRMCommand, VoidMethodResult>
    {
        private readonly IInteractionService _interactionService;
        private readonly IOrderService _orderService;
        private readonly IPlatformRepository _platformRepository;
        private readonly UserManager<User> _userManager;
        private readonly IUserRepository _userRepository;
        private readonly IMediator _mediator;
        private readonly AppSetting _appSetting;
        private readonly ISystemService _systemService;

        public CreateOrdersFromCRMCommandHandler(UserManager<User> userManager, IOrderService orderService, IPlatformRepository platformRepository, IInteractionService interactionService, IUserRepository userRepository, IMediator mediator, AppSetting appSetting, ISystemService systemService)
        {
            _userManager = userManager;
            _orderService = orderService;
            _platformRepository = platformRepository;
            _interactionService = interactionService;
            _userRepository = userRepository;
            _mediator = mediator;
            _appSetting = appSetting;
            _systemService = systemService;
        }

        public async Task<VoidMethodResult> Handle(CreateOrdersFromCRMCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new VoidMethodResult();

            if (request.UsersInfo == null || request.UsersInfo.Count == 0)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                return methodResult;
            }

            if (request.UsersInfo.Any(p => string.IsNullOrEmpty(GetUserName(p).Item1)))
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.Required));
                return methodResult;
            }

            if (request.UsersInfo.Any(p => p.Package != 1 && p.Package != 6 && p.Package != 12))
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.InValidFormat));
                return methodResult;
            }

            if (request.UsersInfo.Any(p => !CheckValueFormat(p.Email, true) || !CheckValueFormat(p.PhoneNumber, false) || !CheckValueFormat(p.FatherPhoneNumber, false) || !CheckValueFormat(p.FatherEmail, true) || !CheckValueFormat(p.MotherPhoneNumber, false) || !CheckValueFormat(p.MotherEmail, true)))
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.InValidFormat));
                return methodResult;
            }

            var platform = await _platformRepository.GetPlatformAsync(EnumPlatformCode.LMS, cancellationToken);
            if (platform == null)
            {
                methodResult.AddErrorBadRequest("Platform null");
                return methodResult;
            }

            var schoolResults = await _systemService.ExecuteListSchoolQueryAsync(new BaseQueryModel());
            var schools = schoolResults.Content?.Result;

            var usersInfo = new List<CreateOrdersFromCRMModel>();

            foreach (var item in request.UsersInfo)
            {
                Microsoft.AspNetCore.Identity.IdentityResult identityStudentResult;

                User? user = new User();

                var userName = GetUserName(item);

                user = await _userManager.Users.Include(p => p.Student).ThenInclude(x => x.ParentStudents).FirstOrDefaultAsync(p => p.UserName.ToLower() == userName.Item1.ToLower() || p.Email.ToLower() == userName.Item1.ToLower() || p.PhoneNumber.ToLower() == userName.Item1.ToLower(), cancellationToken);

                if (user == null)
                {
                    Guid? schoolId = null;
                    string? schoolName = string.Empty;
                    if (!string.IsNullOrEmpty(item.LongPath))
                    {
                        var school = GetSchool(item.LongPath, schools);
                        schoolId = school.Item1;
                        schoolName = school.Item2;
                    }

                    user = new User()
                    {
                        UserName = userName.Item1,
                        Email = userName.Item1.IsValidEmail() ? userName.Item1 : null,
                        EmailConfirmed = userName.Item1.IsValidEmail(),
                        FirstName = item.FirstName,
                        LastName = item.LastName,
                        Birthday = item.Birthday,
                        Gender = item.Gender,
                        PhoneNumber = userName.Item1.IsValidPhoneNumber() ? userName.Item1 : null,
                        PhoneNumberConfirmed = userName.Item1.IsValidPhoneNumber(),
                        Address = item.Address,
                        Student = new Student()
                        {
                            CreatedByParent = false,
                            Occupation = "Student",
                            CourseLevel = EnumCourseLevel.A1,
                            School = schoolName,
                            SchoolId = schoolId,
                        },
                        UserPlatforms = new List<UserPlatform>()
                        {
                              new UserPlatform()
                              {
                                PlatformId = platform.Id
                              }
                        }
                    };

                    await _userRepository.GenerateUserDataAsync(user, EnumRoleRegister.Student);

                    if (!user.IsValid())
                    {
                        methodResult.AddError(user.ErrorMessages);
                        return methodResult;
                    }

                    var passwordGeneratorHelper = new PasswordGeneratorHelper(8, 10, 1, 1, 1, 1);
                    var password = passwordGeneratorHelper.Generate();

                    identityStudentResult = await _userManager.CreateAsync(user, password);
                    if (!identityStudentResult.Succeeded)
                    {
                        methodResult.AddErrorBadRequest(nameof(EnumAuthUserErrorCode.UserFailToCreate), nameof(item.Email), item.Email);
                        return methodResult;
                    }

                    await _userManager.AddToRoleAsync(user, EnumRole.Student.ToString());

                    #region create account parents

                    if ((!string.IsNullOrEmpty(item.FatherEmail) || !string.IsNullOrEmpty(item.FatherPhoneNumber)) && userName.Item2 != 2)
                    {
                        var identityFatherResult = new Microsoft.AspNetCore.Identity.IdentityResult();
                        var fatherPassword = passwordGeneratorHelper.Generate();
                        var father = await CreateAccountParent(user, item.FatherEmail, item.FatherName, item.FatherPhoneNumber, platform, fatherPassword, identityFatherResult, methodResult, true, cancellationToken);
                        if (father == null)
                        {
                            return methodResult;
                        }
                    }

                    if ((!string.IsNullOrEmpty(item.MotherEmail) || !string.IsNullOrEmpty(item.MotherPhoneNumber)) && userName.Item2 != 3)
                    {
                        var identityMotherResult = new Microsoft.AspNetCore.Identity.IdentityResult();
                        var motherPassword = passwordGeneratorHelper.Generate();
                        var mother = await CreateAccountParent(user, item.MotherEmail, item.MotherName, item.MotherPhoneNumber, platform, motherPassword, identityMotherResult, methodResult, false, cancellationToken);
                        if (mother == null)
                        {
                            return methodResult;
                        }
                    }

                    await _userManager.UpdateAsync(user);

                    #endregion create account parents

                    if (user.UserName.IsValidEmail() && !string.IsNullOrEmpty(user.UserName))
                    {
                        var param = new
                        {
                            UserName = user.UserName,
                            Password = password,
                            ContinueLearn = _appSetting.ResourceContent?.LmsWebsiteUrl
                        };
                        await SendMail(user.UserName, param, cancellationToken);
                    }
                }
                usersInfo.Add(new CreateOrdersFromCRMModel
                {
                    UserId = user.Id,
                    FullName = user.FirstName + " " + user.LastName,
                    Email = user.UserName,
                    StudentCode = user.Code,
                    DiscountPercent = item.DiscountPercent,
                    PackageCode = GetPackage(item.Package)
                });
            }
            var createOrdersResult = await _orderService.CreateOrdersFromCRM(new CreateOrdersFromCRMModels()
            {
                UsersInfo = usersInfo
            });
            if (!createOrdersResult.IsSuccessStatusCode)
            {
                methodResult.AddError(createOrdersResult.Error);
                return methodResult;
            }
            return methodResult;
        }

        private static (Guid?, string?) GetSchool(string longPath, IList<Application.Services.SystemService.Model.SchoolModel>? schools)
        {
            var listLocation = SplitString(longPath);
            var paths = listLocation.Skip(listLocation.Length - 2).Select(x => x.Trim());
            var shortPath = string.Join("/", paths);
            var school = schools?.Where(x => !string.IsNullOrEmpty(x.LongPath) && x.LongPath.Contains(shortPath, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
            return (school?.Id, school?.Name);
        }

        private static string[] SplitString(string input)
        {
            string[] words = input.Split('/');
            return words;
        }

        private async Task SendMail(string email, object param, CancellationToken cancellationToken)
        {
            await _mediator.Send(new SenderCommand
            {
                Email = email,
                Subject = SenderSettings.CreateAccountFromCRM,
                Params = param,
                Template = EnumSenderTemplate.CreateAccountFromCRM,
            }, cancellationToken).ConfigureAwait(false);
        }

        private static bool CheckValueFormat(string? value, bool isValidEmail)
        {
            if (string.IsNullOrEmpty(value))
            {
                return true;
            }
            return isValidEmail ? value.IsValidEmail() : value.IsValidPhoneNumber();
        }

        private static EnumPackageCode GetPackage(int package)
        {
            if (package == 1)
            {
                return EnumPackageCode.BASIC;
            }
            else if (package == 6)
            {
                return EnumPackageCode.STANDARD;
            }
            else
            {
                return EnumPackageCode.PREMIUM;
            }
        }

        private static (string?, int) GetUserName(CreateOrdersFromCRMCommandModel model)
        {
            if (!string.IsNullOrEmpty(model.Email))
            {
                return (model.Email, 1);
            }
            else if (!string.IsNullOrEmpty(model.PhoneNumber))
            {
                return (model.PhoneNumber, 1);
            }
            else if (!string.IsNullOrEmpty(model.FatherEmail))
            {
                return (model.FatherEmail, 2);
            }
            else if (!string.IsNullOrEmpty(model.FatherPhoneNumber))
            {
                return (model.FatherPhoneNumber, 2);
            }
            else if (!string.IsNullOrEmpty(model.MotherEmail))
            {
                return (model.MotherEmail, 3);
            }
            else if (!string.IsNullOrEmpty(model.MotherPhoneNumber))
            {
                return (model.MotherPhoneNumber, 3);
            }
            return (null, 0);
        }

        private async Task<User?> CreateAccountParent(User user, string? parentEmail, string? parentName, string? parentPhoneNumber, Platform platform, string password, Microsoft.AspNetCore.Identity.IdentityResult? identityResult, VoidMethodResult methodResult, bool isFather, CancellationToken cancellationToken)
        {
            var userName = !string.IsNullOrEmpty(parentEmail) ? parentEmail : parentPhoneNumber;
            var parent = await _userManager.Users.Include(p => p.Parent).FirstOrDefaultAsync(p => p.UserName.ToLower() == userName.ToLower() || p.PhoneNumber.ToLower() == userName.ToLower() || p.Email.ToLower() == userName.ToLower(), cancellationToken);

            if (parent == null)
            {
                parent = new User()
                {
                    UserName = userName,
                    Email = parentEmail,
                    FirstName = parentName.ParseFullName().FirstName,
                    LastName = parentName.ParseFullName().LastName,
                    EmailConfirmed = !string.IsNullOrEmpty(parentEmail),
                    PhoneNumber = parentPhoneNumber,
                    PhoneNumberConfirmed = !string.IsNullOrEmpty(parentPhoneNumber),
                    Gender = isFather ? EnumGender.Male : EnumGender.Female,
                    Parent = new Parent()
                    {
                        Occupation = "Parent",
                    },
                    UserPlatforms = new List<UserPlatform>()
                                {
                                    new UserPlatform()
                                    {
                                        PlatformId = platform.Id
                                    }
                                }
                };

                await _userRepository.GenerateUserDataAsync(parent, EnumRoleRegister.Parent);

                if (!parent.IsValid())
                {
                    return null;
                }

                identityResult = await _userManager.CreateAsync(parent, password);
                if (!identityResult.Succeeded)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumAuthUserErrorCode.UserFailToCreate));
                    return null;
                }
                await _userManager.AddToRoleAsync(parent, EnumRole.Parent.ToString());
            }
            if (parent.Parent != null && !user.Student.ParentStudents.Any(p => p.ParentId == parent.Parent.Id))
            {
                user.Student?.ParentStudents.Add(new ParentStudent
                {
                    ParentId = parent.Parent.Id,
                });
                await _userManager.UpdateAsync(user);
            }
            return parent;
        }
    }
}
