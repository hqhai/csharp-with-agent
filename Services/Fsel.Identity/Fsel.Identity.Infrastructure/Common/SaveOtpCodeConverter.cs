// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Infrastructure.Common
{
    using Fsel.Identity.Domain.Entities;
    using Fsel.Identity.Domain.Enums;
    using Fsel.Identity.Domain.IRepositories;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Helpers;

    public class SaveOtpCodeConverter
    {
        private readonly IUserOtpCodeRepository _userOtpCodeRepository;
        private const int AddOneCountRetry = 1;

        public SaveOtpCodeConverter(IUserOtpCodeRepository userOtpCodeRepository)
        {
            _userOtpCodeRepository = userOtpCodeRepository;
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
    }
}
