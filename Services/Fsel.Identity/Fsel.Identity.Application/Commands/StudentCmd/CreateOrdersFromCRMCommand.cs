// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Commands.StudentCmd
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Common.Helpers;
    using Fsel.Core.Base.Managers;
    using Fsel.Identity.Application.Services.InteractionService;
    using Fsel.Identity.Application.Services.InteractionService.Models;
    using Fsel.Identity.Application.Services.OrderService;
    using Fsel.Identity.Application.Services.OrderService.Model;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Identity.Domain.Enums.ErrorCodes;
    using Fsel.Identity.Domain.IRepositories;
    using Fsel.Identity.Domain.Models.CommandModels.Students;
    using Fsel.Shared.Enums;
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

        public CreateOrdersFromCRMCommandHandler(UserManager<User> userManager, IOrderService orderService, IPlatformRepository platformRepository, IInteractionService interactionService)
        {
            _userManager = userManager;
            _orderService = orderService;
            _platformRepository = platformRepository;
            _interactionService = interactionService;
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

            if (request.UsersInfo.Any(p => string.IsNullOrEmpty(p.Email) && string.IsNullOrEmpty(p.PhoneNumber)))
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.Required));
                return methodResult;
            }

            if (request.UsersInfo.Any(p => !string.IsNullOrEmpty(p.Email) && !p.Email.IsValidEmail()))
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.InValidFormat));
                return methodResult;
            }

            if (request.UsersInfo.Any(p => !string.IsNullOrEmpty(p.PhoneNumber) && !p.PhoneNumber.IsValidPhoneNumber()))
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.InValidFormat));
                return methodResult;
            }

            if (request.UsersInfo.Any(p => (!string.IsNullOrEmpty(p.FatherEmail) && !p.FatherEmail.IsValidEmail()) || (!string.IsNullOrEmpty(p.MotherEmail) && !p.MotherEmail.IsValidEmail())))
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.InValidFormat));
                return methodResult;
            }

            if (request.UsersInfo.Any(p => (!string.IsNullOrEmpty(p.FatherPhoneNumber) && !p.FatherPhoneNumber.IsValidPhoneNumber()) || (!string.IsNullOrEmpty(p.MotherPhoneNumber) && !p.MotherPhoneNumber.IsValidPhoneNumber())))
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

            var usersInfo = new List<CreateOrdersFromCRMModel>();

            foreach (var item in request.UsersInfo)
            {
                Microsoft.AspNetCore.Identity.IdentityResult identityStudentResult;
                var user = await _userManager.Users.FirstOrDefaultAsync(p => p.UserName.ToLower() == item.Email.ToLower() || p.Email.ToLower() == item.Email.ToLower(), cancellationToken);
                if (user != null)
                {
                    user = new User()
                    {
                        UserName = item.Email,
                        Email = item.Email,
                        PhoneNumber = item.Email,
                        FirstName = item.FirstName,
                        LastName = item.LastName,
                        EmailConfirmed = true,
                        Birthday = item.Birthday,
                        Gender = item.Gender,
                        Student = new Student()
                        {
                            CreatedByParent = false,
                            Occupation = "Student",
                            CourseLevel = EnumCourseLevel.A1,
                            School = item.SchoolCode,
                        },
                        UserPlatforms = new List<UserPlatform>()
                        {
                              new UserPlatform()
                              {
                                PlatformId = platform.Id
                              }
                        }
                    };

                    if (!user.IsValid())
                    {
                        methodResult.AddError(user.ErrorMessages);
                        return methodResult;
                    }

                    var passwordGeneratorHelper = new PasswordGeneratorHelper(6, 10, 1, 1, 1, 1);
                    var password = passwordGeneratorHelper.Generate();

                    identityStudentResult = await _userManager.CreateAsync(user, password);
                    if (!identityStudentResult.Succeeded)
                    {
                        methodResult.AddErrorBadRequest(nameof(EnumAuthUserErrorCode.UserFailToCreate), nameof(item.Email), item.Email);
                        return methodResult;
                    }

                    await _userManager.AddToRoleAsync(user, EnumRole.Student.ToString());

                    if (!string.IsNullOrEmpty(item.FatherEmail))
                    {
                        var identityFatherResult = new Microsoft.AspNetCore.Identity.IdentityResult();
                        var fatherPassword = passwordGeneratorHelper.Generate();
                        var father = await CreateAccountParent(user, item.FatherEmail, item.FatherName, platform, fatherPassword, identityFatherResult, methodResult, cancellationToken);
                        if (father == null)
                        {
                            return methodResult;
                        }
                    }

                    if (!string.IsNullOrEmpty(item.MotherEmail))
                    {
                        var identityMotherResult = new Microsoft.AspNetCore.Identity.IdentityResult();
                        var motherPassword = passwordGeneratorHelper.Generate();
                        var mother = await CreateAccountParent(user, item.MotherEmail, item.MotherName, platform, motherPassword, identityMotherResult, methodResult, cancellationToken);
                        if (mother == null)
                        {
                            return methodResult;
                        }
                    }

                    await _userManager.UpdateAsync(user);

                    var createSurveyResult = await _interactionService.CreateSurvey(new CreateCustomerSurveyCommandModel
                    {
                        Email = user.Email,
                        UserId = user.Id,
                        Answers = new List<CreateSurveyCommandModel>
                        {
                            new CreateSurveyCommandModel
                                {
                                    Id = Guid.Parse("492D8BB9-CDBE-42E7-AA16-35A1915C3621"),
                                    Answer = new { Id = 1,Content = "Google",Image = "gmail-icon.svg"},
                                }
                        }
                    });
                }

                var createOrdersResult = await _orderService.CreateOrdersFromCRM(new CreateOrdersFromCRMModels()
                {
                    UsersInfo = usersInfo
                });
            }
            return methodResult;
        }

        private async Task<User?> CreateAccountParent(User user, string? parentEmail, string? parentName, Platform platform, string password, Microsoft.AspNetCore.Identity.IdentityResult? identityResult, VoidMethodResult methodResult, CancellationToken cancellationToken)
        {
            var parent = await _userManager.Users.Include(p => p.Parent).FirstOrDefaultAsync(p => p.UserName.ToLower() == parentEmail.ToLower() || p.Email.ToLower() == parentEmail.ToLower(), cancellationToken);

            var parseFullName = Shared.Helpers.StringHelper.ParseFullName(parentName);

            parent = new User()
            {
                UserName = parentEmail,
                Email = parentEmail,
                FirstName = !string.IsNullOrEmpty(parseFullName.Item1) ? parseFullName.Item1 : parentName?.Split('@').LastOrDefault(),
                LastName = !string.IsNullOrEmpty(parseFullName.Item2) ? parseFullName.Item2 : parentName?.Split('@').LastOrDefault(),
                EmailConfirmed = true,
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

            if (parent.Parent != null)
            {
                user.Student?.ParentStudents.Add(new ParentStudent
                {
                    ParentId = parent.Parent.Id,
                });
            }

            return parent;
        }
    }
}
