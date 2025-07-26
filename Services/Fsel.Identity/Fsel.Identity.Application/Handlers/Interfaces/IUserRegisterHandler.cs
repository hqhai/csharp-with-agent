// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Handlers.Interfaces
{
    using Fsel.Identity.Domain.Models.CommandModels.OpenId;

    public interface IUserRegisterHandler
    {
        Task<(bool, string)> TempRegisterUserAsync(UserRegisterModel userRegisterModel);

        Task<(bool, string)> SendRegisterOtpAsync(string phoneNumber, OtpProviderType otpProviderType = OtpProviderType.Sms);

        Task<(bool, string)> VerifyUserAsync(string phoneNumber, string otpCode);

        Task<(bool, string)> CreateUserAsync(string phoneNumber, string otpCode);
    }
}
