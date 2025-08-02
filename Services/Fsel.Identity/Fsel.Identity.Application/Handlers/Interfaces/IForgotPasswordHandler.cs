// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Handlers.Interfaces
{
    public interface IForgotPasswordHandler
    {
        Task<(bool, OtpSessionInfo)> SendOtpAsync(string identity, OtpProviderType otpProviderType);

        Task<(bool, OtpSessionInfo)> VerifyOtpAsync(string identity, string otpCode);

        Task<string> GetResetPasswordToken(string identity);
    }
}
