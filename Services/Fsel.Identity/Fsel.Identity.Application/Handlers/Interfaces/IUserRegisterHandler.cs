// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Handlers.Interfaces
{
    using Fsel.Identity.Domain.Models.CommandModels.OpenId;
    using Microsoft.AspNetCore.Identity;

    public interface IUserRegisterHandler
    {
        Task<bool> TempRegisterUserAsync(UserRegisterModel userRegisterModel);

        Task<(bool, OtpSessionInfo)> SendRegisterOtpAsync(string phoneNumber, OtpProviderType otpProviderType = OtpProviderType.Zalo);

        Task<(bool, OtpSessionInfo)> VerifyUserAsync(string phoneNumber, string otpCode);

        Task<IdentityResult> CreateUserAsync(string phoneNumber, string otpCode);
    }
}
