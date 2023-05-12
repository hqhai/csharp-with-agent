// Copyright (c) Atlantic. All rights reserved.

using Fsel.Common.ActionResults;
using Fsel.Identity.Domain.Entities;
using Fsel.Identity.Domain.Enums;
using Fsel.Identity.Domain.Enums.ErrorCodes;
using Fsel.Identity.Domain.Models.CommandModels.Auths;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Fsel.Identity.Application.Commands.AuthCmd
{
    public class ComfirmOTPResetPasswordCommand : ResetPasswordCommandModel, IRequest<MethodResult<bool>>
    {
    }

    public class ComfirmOTPResetPasswordCommandHandler : IRequestHandler<ComfirmOTPResetPasswordCommand, MethodResult<bool>>
    {
        private readonly UserManager<User> _userManager;

        public ComfirmOTPResetPasswordCommandHandler(UserManager<User> userManager)
        {
            _userManager = userManager;
        }

        public async Task<MethodResult<bool>> Handle(ComfirmOTPResetPasswordCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<bool> methodResult = new MethodResult<bool>();

            if (string.IsNullOrEmpty(request.NewPassword))
            {
                methodResult.AddErrorBadRequest(nameof(EnumAuthErrorCode.PasswordNotEmpty), nameof(request.NewPassword));
                return methodResult;
            }
            if (string.IsNullOrEmpty(request.Otp))
            {
                methodResult.AddErrorBadRequest(nameof(EnumAuthErrorCode.OtpNull), nameof(request.Otp));
                return methodResult;
            }

            var user = await _userManager.Users.Include(x => x.UserOtpCodes)
                                .FirstOrDefaultAsync(x => x.UserOtpCodes.Where(x => x.Status == EnumStatusUser.New).Select(x => x.OTPCode).Contains(request.Otp), cancellationToken);

            if (user == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumAuthErrorCode.OtpNotExist), nameof(request.Otp), request.Otp);
                return methodResult;
            }

            var hashPassword = _userManager.PasswordHasher.HashPassword(user, request.NewPassword);
            user.PasswordHash = hashPassword;
            await _userManager.UpdateAsync(user);

            methodResult.StatusCode = StatusCodes.Status200OK;
            methodResult.Result = true;
            return methodResult;
        }
    }
}
