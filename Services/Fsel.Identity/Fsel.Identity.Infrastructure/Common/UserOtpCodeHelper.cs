// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Infrastructure.Common
{
    using System.Threading;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Helpers;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Identity.Domain.Enums;
    using Fsel.Identity.Domain.Enums.ErrorCodes;
    using Fsel.Identity.Domain.IRepositories;
    using Fsel.Identity.Domain.Models.CommandModels.UserOtpCodes;
    using Fsel.Identity.Infrastructure.ValueSettings;
    using Microsoft.EntityFrameworkCore;

    public class UserOtpCodeHelper
    {
        private readonly IUserOtpCodeRepository _userOtpCodeRepository;
        private readonly AppSetting _appSetting;

        public UserOtpCodeHelper(IUserOtpCodeRepository userOtpCodeRepository, AppSetting appSetting)
        {
            _userOtpCodeRepository = userOtpCodeRepository;
            _appSetting = appSetting;
        }

        public async Task<MethodResult<UserOtpCode>> ValidateOtp(ConfirmOtpCommandModel request, CancellationToken cancellationToken)
        {
            MethodResult<UserOtpCode> methodResult = new MethodResult<UserOtpCode>();
            ArgumentNullException.ThrowIfNull(request);

            var userOtpCode = await _userOtpCodeRepository.Queryable
                                  .FirstOrDefaultAsync(x => x.Status == EnumOtpCodeStatus.New && !x.IsDeleted && x.OTPCode == request.Otp, cancellationToken);
            if (!string.IsNullOrEmpty(request.Email) && _appSetting.Otp != null && request.Otp == _appSetting.Otp.Key && _appSetting.Otp.ByPassOtp)
            {
                if (!request.Email.IsValidEmail())
                {
                    methodResult.AddErrorBadRequest(nameof(EnumAuthUserErrorCode.EmailIsNotValid), nameof(request.Email));
                    return methodResult;
                }
                userOtpCode = await _userOtpCodeRepository.Queryable.Include(x => x.User)
                                   .FirstOrDefaultAsync(x => x.User != null && x.Status == EnumOtpCodeStatus.New && !x.IsDeleted && x.User.Email == request.Email, cancellationToken);
            }
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
            methodResult.Result = userOtpCode;
            return methodResult;
        }
    }
}
