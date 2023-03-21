// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Enums.ErrorCodes
{
    public enum EnumAuthErrorCode
    {
        /// <summary>
        /// Old Password cannot empty
        /// </summary>
        OldPassWordNotEmpty,

        /// <summary>
        /// Password cannot empty
        /// </summary>
        PasswordNotEmpty,

        /// <summary>
        /// Confirm Password cannot empty
        /// </summary>
        ConfirmPasswordNotEmpty,

        /// <summary>
        /// Email does not exist
        /// </summary>
        EmailNotExist,

        /// <summary>
        /// Old password is incorrect
        /// </summary>
        OldPasswordIncorrect,

        /// <summary>
        /// UserName cannot empty
        /// </summary>
        AU06V,

        /// <summary>
        /// PhoneNumber does not exist
        /// </summary>
        AU07V,

        /// <summary>
        /// Duplicate email, please select another email
        /// </summary>
        AU08V,

        /// <summary>
        /// Duplicate PhoneNumber, please select another PhoneNumber
        /// </summary>
        AU09V,

        /// <summary>
        /// Email verified fail
        /// </summary>
        EmailVerifiedFail,

        /// <summary>
        /// Error while generating reset token
        /// </summary>
        ErrorResetToken,

        /// <summary>
        /// Error while generating reset Password
        /// </summary>
        ErrorResetPassword,

        /// <summary>
        /// Username and Password cannot empty
        /// </summary>
        UserNameAndPasswordNotEmpty,

        /// <summary>
        /// Username and Password  is incorrect
        /// </summary>
        UserNameAndPasswordIncorrect,

        /// <summary>
        /// Invalid token
        /// </summary>
        InvalidToken,

        /// <summary>
        /// Access token has not yet expired
        /// </summary>
        AccessTokenNotYetExpired,

        /// <summary>
        /// Refresh token has expired
        /// </summary>
        RefreshTokenExpired,

        /// <summary>
        /// Sign up fail
        /// </summary>
        SignUpFail,

        /// <summary>
        /// User Failed to Create
        /// </summary>
        AU10ER,

        /// <summary>
        /// Send Auth erorr
        /// </summary>
        AU11ER,

        /// <summary>
        /// User was active
        /// </summary>
        AU12ER,

        /// <summary>
        /// Not Send Email
        /// </summary>
        AU13ER,

        /// <summary>
        /// Account already exists
        /// </summary>
        AU14ER,

        /// <summary>
        /// Invalid OTP, please try again
        /// </summary>
        AU15ER,

        /// <summary>
        /// OTP has expired, please click on the Resend code button below to receive a new OTP
        /// </summary>
        AU16ER
    }
}