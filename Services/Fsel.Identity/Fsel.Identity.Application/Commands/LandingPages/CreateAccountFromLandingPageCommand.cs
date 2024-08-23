// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Commands.LandingPages
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Common.Helpers;
    using Fsel.Core.Base.Managers;
    using Fsel.Identity.Application.Commands.UserCmd;
    using Fsel.Identity.Application.Services.InteractionService;
    using Fsel.Identity.Application.Services.InteractionService.Models;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Identity.Domain.Enums;
    using Fsel.Identity.Domain.Enums.ErrorCodes;
    using Fsel.Identity.Domain.IRepositories;
    using Fsel.Identity.Domain.Models.CommandModels.LandingPages;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class CreateAccountFromLandingPageCommand : CreateAccountFromLandingPageCommandModel, IRequest<MethodResult<bool>>
    {
    }

    public class CreateAccountFromLandingPageCommandHandler : IRequestHandler<CreateAccountFromLandingPageCommand, MethodResult<bool>>
    {
        private readonly UserManager<User> _userManager;
        private readonly IPlatformRepository _platformRepository;
        private readonly IInteractionService _interactionService;
        private readonly IMediator _mediator;

        public CreateAccountFromLandingPageCommandHandler(UserManager<User> userManager, IPlatformRepository platformRepository, IInteractionService interactionService, IMediator mediator)
        {
            _userManager = userManager;
            _platformRepository = platformRepository;
            _interactionService = interactionService;
            _mediator = mediator;
        }

        public async Task<MethodResult<bool>> Handle(CreateAccountFromLandingPageCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<bool>();

            if (string.IsNullOrEmpty(request.FullName) || string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.PhoneNumber) || string.IsNullOrEmpty(request.Password))
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                return methodResult;
            }
            User? user = null;
            if (!request.PhoneNumber.IsValidPhoneNumber())
            {
                methodResult.AddErrorBadRequest(nameof(EnumAuthUserErrorCode.PhoneNumberIsNotValid), nameof(request.PhoneNumber));
                return methodResult;
            }
            user = await _userManager.Users.FirstOrDefaultAsync(x => x.PhoneNumber.ToLower() == request.PhoneNumber.ToLower(), cancellationToken: cancellationToken);
            if (user != null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumAuthUserErrorCode.DuplicatePhoneNumber), nameof(request.PhoneNumber), request.PhoneNumber);
                return methodResult;
            }
            if (!request.Email.IsValidEmail())
            {
                methodResult.AddErrorBadRequest(nameof(EnumAuthUserErrorCode.EmailIsNotValid), nameof(request.Email));
                return methodResult;
            }
            user = await _userManager.Users.FirstOrDefaultAsync(x => x.Email.ToLower() == request.Email.ToLower() || x.UserName.ToLower() == request.Email.ToLower(), cancellationToken: cancellationToken);
            if (user != null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumAuthUserErrorCode.DuplicateEmail), nameof(request.Email), request.Email);
                return methodResult;
            }

            var platform = await _platformRepository.GetPlatformAsync(EnumPlatformCode.LMS, cancellationToken);
            if (platform == null)
            {
                methodResult.AddErrorBadRequest("Platform null");
                return methodResult;
            }
            Microsoft.AspNetCore.Identity.IdentityResult identityStudentResult;
            user = new User()
            {
                UserName = request.Email,
                FullName = request.FullName,
                Email = request.Email,
                EmailConfirmed = true,
                PhoneNumber = request.PhoneNumber,
                PhoneNumberConfirmed = false,
                Human = new Human()
                {
                    FullName = request.FullName,
                    Birthday = request.Birthday,
                    PhoneNumber = request.PhoneNumber,
                    Email = request.Email,
                    Student = new Student()
                    {
                        Occupation = "Student",
                        CourseLevel = EnumCourseLevel.A1,
                        CreatedByParent = false
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

            var passwordValidator = new Microsoft.AspNetCore.Identity.PasswordValidator<User>();
            var validPassword = await passwordValidator.ValidateAsync(_userManager, user, request.Password);
            if (!validPassword.Succeeded)
            {
                methodResult.AddErrorBadRequest(nameof(EnumAuthUserErrorCode.PasswordIsNotValid));
                return methodResult;
            }

            if (!user.IsValid())
            {
                methodResult.AddError(user.ErrorMessages);
                return methodResult;
            }

            identityStudentResult = await _userManager.CreateAsync(user, request.Password);
            if (!identityStudentResult.Succeeded)
            {
                methodResult.AddErrorBadRequest(nameof(EnumAuthUserErrorCode.UserFailToCreate));
                return methodResult;
            }
            await _userManager.AddToRoleAsync(user, EnumRole.Student.ToString());

            var updateCode = await _mediator.Send(new UpdateCodeStudentCommand { UserId = user.Id, Gender = EnumGender.Male, Birthday = user.Human.Birthday }, cancellationToken);
            if (!updateCode.IsOK)
            {
                methodResult.AddErrorBadRequest(updateCode.ErrorMessages);
                return methodResult;
            }

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

            methodResult.Result = true;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
