// Copyright (c) Atlantic. All rights reserved.

using Fsel.Common.ActionResults;
using Fsel.Identity.Domain.Entities;
using Fsel.Identity.Domain.Enums;
using Fsel.Identity.Domain.Enums.ErrorCodes;
using Fsel.Identity.Domain.IRepositories;
using Fsel.Identity.Domain.Models.CommandModels.Auths;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Fsel.Identity.Application.Commands.AuthCmd
{
    public class ComfirmOTPResetPasswordCommand : ComfirmOTPResetPasswordCommandModel, IRequest<MethodResult<bool>>
    {
    }

    public class ComfirmOTPResetPasswordCommandHandler : IRequestHandler<ComfirmOTPResetPasswordCommand, MethodResult<bool>>
    {
        private readonly UserManager<User> _userManager;
        private readonly IUserOtpCodeRepository _userOtpCodeRepository;

        public ComfirmOTPResetPasswordCommandHandler(UserManager<User> userManager, IUserOtpCodeRepository userOtpCodeRepository)
        {
            _userManager = userManager;
            _userOtpCodeRepository = userOtpCodeRepository;
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

            var userOtpCode = await _userOtpCodeRepository.Queryable
                       .FirstOrDefaultAsync(x => x.UserId == user!.Id && x.Status == EnumStatusUser.New && !x.IsDeleted && x.OTPCode == request.Otp, cancellationToken);
            if (userOtpCode == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumAuthErrorCode.InvalidOTP), nameof(request.Otp), request.Otp);
                return methodResult;
            }

            if (DateTime.Compare(DateTime.Now, userOtpCode.ExpiredTime) > 0)
            {
                methodResult.AddErrorBadRequest(nameof(EnumAuthErrorCode.OTPExpired), nameof(request.Otp), request.Otp);
                return methodResult;
            }

            userOtpCode.Status = EnumStatusUser.Verified;
            _userOtpCodeRepository.Update(userOtpCode);
            await _userOtpCodeRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
            if (!user.EmailConfirmed)
            {
                var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                await _userManager.ConfirmEmailAsync(user, token);
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
