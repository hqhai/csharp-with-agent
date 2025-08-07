// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Enums.ErrorCodes
{
    public enum EnumAuthUserErrorCode
    {
        /// <summary>
        /// Confirm Password cannot empty
        /// </summary>
        ConfirmPasswordNotEmpty,

        /// <summary>
        /// The new password matches the old password
        /// </summary>
        NewPasswordMatchOldPassword,

        /// <summary>
        /// User does not exist by code
        /// </summary>
        UserNotExistByCode,

        /// <summary>
        /// Old password is incorrect
        /// </summary>
        OldPasswordIncorrect,

        /// <summary>
        /// Duplicate email, please select another email
        /// </summary>
        DuplicateEmail,

        /// <summary>
        /// Duplicate PhoneNumber, please select another PhoneNumber
        /// </summary>
        DuplicatePhoneNumber,

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
        UserFailToCreate,

        /// <summary>
        /// Send Auth erorr
        /// </summary>
        SendAuthErorr,

        /// <summary>
        /// User was active
        /// </summary>
        UserActive,

        /// <summary>
        /// Error Sending Email
        /// </summary>
        ErrorSendEmail,

        /// <summary>
        /// Invalid OTP, please try again
        /// </summary>
        InvalidOTP,

        /// <summary>
        /// OTP has expired, please click on the Resend code button below to receive a new OTP
        /// </summary>
        OTPExpired,

        /// <summary>
        /// Survey Called Error
        /// </summary>
        SurveyCalledError,

        /// <summary>
        /// Account has been locked
        /// </summary>
        AccountHasBeenLocked,

        /// <summary>
        /// Packageids entered is incorrect
        /// </summary>
        PackageIdsEnteredIsIncorrect,

        /// <summary>
        /// User is not on any platform
        /// </summary>
        UserIsNotOnAnyPlatform,

        /// <summary>
        /// Email is not valid
        /// </summary>
        EmailIsNotValid,

        /// <summary>
        /// PhoneNumber is not valid
        /// </summary>
        PhoneNumberIsNotValid,

        /// <summary>
        /// Password is not valid
        /// </summary>
        PasswordIsNotValid,

        /// <summary>
        /// Password is incorrect
        /// </summary>
        PasswordIncorrect,

        /// <summary>
        /// Account has been cut off
        /// </summary>
        AccountHasBeenCutOff,

        OtpBlockInMinutes,

        OtpTryResendAfterMinutes,

        OtpTryResendAfterSeconds,

        RegisterExpired
    }
}
