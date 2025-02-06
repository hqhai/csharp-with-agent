// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Commands.UserOtpCodeCmd
{
    using System;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base.Managers;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Identity.Domain.Enums;
    using Fsel.Identity.Domain.Enums.ErrorCodes;
    using Fsel.Identity.Domain.Models.CommandModels.UserOtpCodes;
    using Fsel.Identity.Infrastructure.ValueSettings;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Enums.ErrorCodes;
    using Kros.Extensions;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class CheckOtpCommand : ConfirmOtpCommandModel, IRequest<MethodResult<bool>>
    {
    }

    public class CheckOtpCommandHandler : IRequestHandler<CheckOtpCommand, MethodResult<bool>>
    {
        private readonly UserManager<User> _userManager;
        private readonly AppSetting _appSetting;

        public CheckOtpCommandHandler(UserManager<User> userManager, AppSetting appSetting)
        {
            _userManager = userManager;
            _appSetting = appSetting;
        }

        public async Task<MethodResult<bool>> Handle(CheckOtpCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<bool>();

            if (!request.Email.IsNullOrEmpty())
            {
                var user = await _userManager.Users
                                             .Include(p => p.UserOtpCodes)
                                             .FirstOrDefaultAsync(p => p.Email.Trim().ToLower() == request.Email.Trim().ToLower(), cancellationToken);
                if (user == null)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumOTPCodeErrorCode.UserDoesNotExist), nameof(request.PhoneNumber), request.PhoneNumber);
                    return methodResult;
                }

                if (_appSetting.Otp != null && request.Otp != _appSetting.Otp.ByPassOtpValue && !_appSetting.Otp.IsByPassOtp)
                {
                    var userOtpCode = user.UserOtpCodes.FirstOrDefault(p => p.Type == EnumUserOtpCodeType.Email && p.Status == EnumOtpCodeStatus.New && p.OTPCode == request.Otp);
                    if (userOtpCode == null)
                    {
                        methodResult.AddErrorBadRequest(nameof(EnumAuthUserErrorCode.InvalidOTP), nameof(request.Otp), request.Otp);
                        return methodResult;
                    }
                    if (request.IsCheckExpiredTime && DateTime.Compare(DateTime.UtcNow, userOtpCode.ExpiredTime) > 0)
                    {
                        methodResult.AddErrorBadRequest(nameof(EnumAuthUserErrorCode.OTPExpired), nameof(request.Otp), request.Otp);
                        return methodResult;
                    }
                }
            }

            else if (!request.PhoneNumber.IsNullOrEmpty())
            {
                var user = await _userManager.Users
                                             .Include(p => p.UserOtpCodes)
                                             .FirstOrDefaultAsync(p => p.UserName == request.PhoneNumber, cancellationToken);

                if (user == null)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumOTPCodeErrorCode.UserDoesNotExist), nameof(request.PhoneNumber), request.PhoneNumber);
                    return methodResult;
                }

                if (_appSetting.Otp != null && request.Otp != _appSetting.Otp.ByPassOtpValue && !_appSetting.Otp.IsByPassOtp)
                {
                    var lastOTP = user.UserOtpCodes.FirstOrDefault(p => p.Type == EnumUserOtpCodeType.SMS && p.Status == EnumOtpCodeStatus.New);
                    if (lastOTP == null)
                    {
                        methodResult.AddErrorBadRequest(nameof(EnumOTPCodeErrorCode.OTPNotSentYet), nameof(request.Otp), request.Otp);
                        return methodResult;
                    }

                    if (lastOTP.OTPCode != request.Otp)
                    {
                        methodResult.AddErrorBadRequest(nameof(EnumOTPCodeErrorCode.WrongOTP), nameof(request.Otp), request.Otp);
                        return methodResult;
                    }
                }
            }

            methodResult.Result = true;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
