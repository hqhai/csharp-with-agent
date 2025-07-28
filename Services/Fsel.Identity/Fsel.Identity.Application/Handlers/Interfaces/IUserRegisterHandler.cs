// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Handlers.Interfaces
{
    using Fsel.Identity.Domain.Models.CommandModels.OpenId;

    public interface IUserRegisterHandler
    {
        Task<(bool, KeyValuePair<string, string>?)> TempRegisterUserAsync(UserRegisterModel userRegisterModel);

        Task<(bool, KeyValuePair<string, string>?)> SendRegisterOtpAsync(string phoneNumber, OtpProviderType otpProviderType = OtpProviderType.Sms);

        Task<(bool, KeyValuePair<string, string>?)> VerifyUserAsync(string phoneNumber, string otpCode);

        Task<(bool, KeyValuePair<string, string>?)> CreateUserAsync(string phoneNumber, string otpCode);
    }
}
