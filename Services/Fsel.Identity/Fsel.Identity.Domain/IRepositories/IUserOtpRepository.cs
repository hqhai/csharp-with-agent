// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.IRepositories
{
    using Fsel.Core.Base.Interfaces;
    using Fsel.Identity.Domain.Entities;

    public interface IUserOtpRepository : IRepository<UserOtp>
    {
        Task<UserOtp?> GetUserOtpCodeAsync(string? otpCode, string? email);
    }
}
