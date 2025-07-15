// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Commands.StudentCmd
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Common.Helpers;
    using Fsel.Core.Base.Managers;
    using Fsel.Identity.Application.Commands.UserCmd;
    using Fsel.Identity.Application.Commands.UserReferrals;
    using Fsel.Identity.Application.Queries.UserReferrals;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Identity.Domain.Enums.ErrorCodes;
    using Fsel.Identity.Domain.IRepositories;
    using Fsel.Identity.Domain.Models.CommandModels.Students;
    using Fsel.Shared.Enums;
    using MediatR;

    public class CreateStudentByAdminCommand : CreateStudentByAdminCommandModel, IRequest<MethodResult<User>>
    {
    }

    public class CreateStudentByAdminCommandHandler : IRequestHandler<CreateStudentByAdminCommand, MethodResult<User>>
    {
        private readonly UserManager<User> _userManager;
        private readonly IPlatformRepository _platformRepository;
        private const string DefaultPassword = "Fsel@2024";
        private readonly IMediator _mediator;

        public CreateStudentByAdminCommandHandler(UserManager<User> userManager, IPlatformRepository platformRepository, IMediator mediator)
        {
            _userManager = userManager;
            _platformRepository = platformRepository;
            _mediator = mediator;
        }

        public async Task<MethodResult<User>> Handle(CreateStudentByAdminCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<User>();

            if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.FullName))
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.Required));
                return methodResult;
            }

            if (!request.Email.IsValidEmail())
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.InValidFormat));
                return methodResult;
            }

            if (!string.IsNullOrEmpty(request.ReferralCode))
            {
                var referral = await _mediator.Send(new CheckReferralCodeQuery() { ReferralCode = request.ReferralCode }, cancellationToken);
                if (referral.Result == null)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                    return methodResult;
                }
            }

            var platform = await _platformRepository.GetPlatformAsync(EnumPlatformCode.LMS, cancellationToken);
            if (platform == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                return methodResult;
            }

            Microsoft.AspNetCore.Identity.IdentityResult identityStudentResult;

            var user = new User()
            {
                UserName = request.Email,
                Email = request.Email,
                FullName = request.FullName,
                PhoneNumber = request.PhoneNumber,
                EmailConfirmed = true,
                Human = new Human()
                {
                    FullName = request.FullName,
                    PhoneNumber = request.PhoneNumber,
                    Birthday = request.DateOfBirth,
                    Email = request.Email,
                    Student = new Student()
                    {
                        CreatedByParent = false,
                        Occupation = "Student",
                        School = request.School,
                        SchoolId = request.SchoolId,
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
            if (!user.IsValid())
            {
                methodResult.AddErrorBadRequest(user.ErrorMessages);
                return methodResult;
            }
            if (user.Human != null && !user.Human.IsValid())
            {
                methodResult.AddErrorBadRequest(user.Human.ErrorMessages);
                return methodResult;
            }

            if (user.Human?.Student != null && !user.Human.Student.IsValid())
            {
                methodResult.AddErrorBadRequest(user.Human.Student.ErrorMessages);
                return methodResult;
            }

            identityStudentResult = await _userManager.CreateAsync(user, DefaultPassword);
            if (!identityStudentResult.Succeeded)
            {
                methodResult.AddErrorBadRequest(nameof(EnumAuthUserErrorCode.UserFailToCreate));
                return methodResult;
            }
            await _userManager.AddToRoleAsync(user, EnumRole.Student.ToString());

            var updateCode = await _mediator.Send(new UpdateCodeStudentCommand { UserId = user.Id, Gender = EnumGender.Male, Birthday = user.Human.Birthday, SchoolName = user.Human.Student.School }, cancellationToken);
            if (!updateCode.IsOK)
            {
                methodResult.AddErrorBadRequest(updateCode.ErrorMessages);
                return methodResult;
            }

            methodResult.Result = user;

            if (!string.IsNullOrEmpty(request.ReferralCode))
            {
                var updateReferralCodeResult = await _mediator.Send(new CreateUserReferralCommand { ReferralCode = request.ReferralCode, ReceiverId = user.Id }, cancellationToken).ConfigureAwait(false);
                if (!updateReferralCodeResult.IsOK)
                {
                    methodResult.AddErrorBadRequest(updateReferralCodeResult.ErrorMessages);
                    return methodResult;
                }
            }

            await _mediator.Send(new SendMailCreateUserCommand() { UserName = request.Email, Password = DefaultPassword }, cancellationToken);

            return methodResult;
        }
    }
}
