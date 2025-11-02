// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.IRepositories
{
    using Fsel.Core.Base.Interfaces;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Shared.Enums;

    public interface IUserOtpCodeRepository : IRepository<UserOtpCode>
    {
        Task<UserOtpCode?> GetUserOtpCodeAsync(string? otpCode, string? email, string? phoneNumber, EnumUserOtpCodeType otpCodeType);

        Task<UserOtpCode?> GetUserOtpCodeAsync(string? otpCode, string? phoneNumber);
    }
}
