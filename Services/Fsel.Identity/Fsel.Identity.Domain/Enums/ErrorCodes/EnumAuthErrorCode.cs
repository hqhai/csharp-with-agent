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
    }
}
