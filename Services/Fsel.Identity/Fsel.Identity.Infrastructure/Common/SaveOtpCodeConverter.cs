// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Infrastructure.Common
{
    using Fsel.Common.ActionResults;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Identity.Domain.Enums;
    using Fsel.Identity.Domain.Enums.ErrorCodes;
    using Fsel.Identity.Domain.IRepositories;
    using Fsel.Identity.Domain.Models.CommandModels.UserOtpCodes;
    using Fsel.Identity.Infrastructure.ValueSettings;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Helpers;

    public class SaveOtpCodeConverter
    {
        private readonly IUserOtpCodeRepository _userOtpCodeRepository;
        private readonly AppSetting _appSetting;
        private const int AddOneCountRetry = 1;

        public SaveOtpCodeConverter(IUserOtpCodeRepository userOtpCodeRepository, AppSetting appSetting)
        {
            _userOtpCodeRepository = userOtpCodeRepository;
            _appSetting = appSetting;
        }

        public async Task<string> SaveOTpCodeBySmsCommand(UserOtpCode? lastOTP, Guid userId, CancellationToken cancellationToken)
        {
            var otp = NumberHelper.GetRandomCode();

            if (lastOTP == null)
            {
                lastOTP = new UserOtpCode
                {
                    UserId = userId,
                    OTPCode = otp,
                    Status = EnumOtpCodeStatus.New,
                    Type = EnumUserOtpCodeType.SMS,
                    RetryCount = AddOneCountRetry,
                    ExpiredTime = DateTime.MaxValue,
                };
                _userOtpCodeRepository.Add(lastOTP);
            }
            else
            {
                lastOTP.OTPCode = otp;
                lastOTP.RetryCount += AddOneCountRetry;
                _userOtpCodeRepository.Update(lastOTP);
            }

            await _userOtpCodeRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return otp;
        }

        public async Task<MethodResult<bool>> CheckOtp(ConfirmOtpCommandModel request, User user, EnumUserOtpCodeType otpCodeType)
        {
            ArgumentNullException.ThrowIfNull(user);
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<bool> methodResult = new MethodResult<bool>();

            if (_appSetting.Otp != null && request.Otp == _appSetting.Otp.ByPassOtpValue && _appSetting.Otp.IsByPassOtp)
            {
                methodResult.Result = true;
                return methodResult;
            }
            else
            {
                var userOtpCode = user.UserOtpCodes.FirstOrDefault(p => p.Type == otpCodeType && p.Status == EnumOtpCodeStatus.New && p.OTPCode == request.Otp);
                if (userOtpCode == null)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumAuthUserErrorCode.InvalidOTP), nameof(request.Otp), request.Otp);
                    return methodResult;
                }

                if (otpCodeType == EnumUserOtpCodeType.Email && request.IsCheckExpiredTime && DateTime.Compare(DateTime.UtcNow, userOtpCode.ExpiredTime) > 0)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumAuthUserErrorCode.OTPExpired), nameof(request.Otp), request.Otp);
                    return methodResult;
                }
            }

            methodResult.Result = true;
            return await Task.FromResult(methodResult);
        }
    }
}
