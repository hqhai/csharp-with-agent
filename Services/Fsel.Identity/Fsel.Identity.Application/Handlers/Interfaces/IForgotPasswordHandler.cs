// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Handlers.Interfaces
{
    public interface IForgotPasswordHandler
    {
        Task<(bool, KeyValuePair<string, string>?)> SendOtpAsync(string identity, OtpProviderType otpProviderType = OtpProviderType.Sms);

        Task<(bool, KeyValuePair<string, string>?)> VerifyOtpAsync(string identity, string otpCode);

        Task<string> GetResetPasswordToken(string identity);
    }
}
