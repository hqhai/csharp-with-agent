// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Commands.StudentCmd
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Common.Helpers;
    using Fsel.Core.Base.Managers;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Identity.Domain.Enums;
    using Fsel.Identity.Domain.Enums.ErrorCodes;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Enums.ErrorCodes;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class UpdatePasswordForUserEventHaNoiCommand : IRequest<MethodResult<bool>>
    {
        public string? Password { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }
        public string? ParentEmail { get; set; }
        public string? ParentPhoneNumber { get; set; }
        public DateTime Birthday { get; set; }
    }

    public class UpdatePasswordForUserEventHaNoiCommandHandle : IRequestHandler<UpdatePasswordForUserEventHaNoiCommand, MethodResult<bool>>
    {
        private readonly UserManager<User> _userManager;

        public UpdatePasswordForUserEventHaNoiCommandHandle(UserManager<User> userManager)
        {
            _userManager = userManager;
        }

        public async Task<MethodResult<bool>> Handle(UpdatePasswordForUserEventHaNoiCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<bool>();

            if (string.IsNullOrEmpty(request.PhoneNumber) || string.IsNullOrEmpty(request.Password))
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.Required));
                return methodResult;
            }

            if (!request.PhoneNumber.IsValidPhoneNumber())
            {
                methodResult.AddErrorBadRequest(ErrorMassageSetting.InvalidPhoneNumberVN);
                return methodResult;
            }

            if (!request.Email.IsValidEmail())
            {
                methodResult.AddErrorBadRequest(ErrorMassageSetting.InvalidEmailVN);
                return methodResult;
            }

            if (!string.IsNullOrEmpty(request.ParentEmail) && !request.ParentEmail.IsValidEmail())
            {
                methodResult.AddErrorBadRequest(ErrorMassageSetting.InvalidEmailVN);
                return methodResult;
            }

            var user = await _userManager.Users.Include(p => p.UserOtpCodes).Include(p => p.Human).ThenInclude(p => p.Student).FirstOrDefaultAsync(p => p.UserName == request.PhoneNumber, cancellationToken);
            if (user == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(user), request.PhoneNumber);
                return methodResult;
            }

            if (await _userManager.Users.AnyAsync(p => p.Id != user.Id && p.Email == request.Email, cancellationToken))
            {
                methodResult.AddErrorBadRequest(nameof(EnumOTPCodeErrorCode.EmailDoesNotExist), nameof(request.Email), request.Email);
                return methodResult;
            }

            if (user.Status == EnumUserStatus.Inactive)
            {
                methodResult.AddErrorBadRequest(nameof(EnumOTPCodeErrorCode.PendingVerification), nameof(request.PhoneNumber), request.PhoneNumber);
                return methodResult;
            }

            var lastOTP = user.UserOtpCodes.Where(p => p.Type == EnumUserOtpCodeType.SMS).OrderByDescending(p => p.CreatedDate).FirstOrDefault();

            if (lastOTP == null || lastOTP.Status != EnumOtpCodeStatus.Verified)
            {
                methodResult.AddErrorBadRequest(nameof(EnumOTPCodeErrorCode.OTPNotVerified));
                return methodResult;
            }
            var passwordValidator = new Microsoft.AspNetCore.Identity.PasswordValidator<User>();
            var validPassword = await passwordValidator.ValidateAsync(_userManager, user, request.Password);
            if (!validPassword.Succeeded)
            {
                methodResult.AddErrorBadRequest(nameof(EnumAuthUserErrorCode.PasswordIsNotValid));
                return methodResult;
            }

            var hashPassword = _userManager.PasswordHasher.HashPassword(user, request.Password);
            user.PasswordHash = hashPassword;
            user.EmailConfirmed = true;
            user.PhoneNumberConfirmed = true;
            user.Email = request.Email;
            user.Human!.Email = request.Email;
            user.Human.Birthday = request.Birthday;
            user.Human.Student!.ParentEmail = request.ParentEmail;
            user.Human.Student.ParentPhoneNumber = request.ParentPhoneNumber;
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
            await _userManager.UpdateAsync(user);

            methodResult.StatusCode = StatusCodes.Status200OK;
            methodResult.Result = true;
            return methodResult;
        }
    }
}
