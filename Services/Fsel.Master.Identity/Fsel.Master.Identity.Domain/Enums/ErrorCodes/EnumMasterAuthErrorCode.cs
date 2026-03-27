// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Master.Identity.Domain.Enums.ErrorCodes
{
    public enum EnumMasterAuthErrorCode
    {
        /// <summary>
        /// Username and Password cannot empty
        /// </summary>
        UserNameAndPasswordNotEmpty,

        /// <summary>
        /// Username and Password is incorrect
        /// </summary>
        UserNameAndPasswordIncorrect,

        /// <summary>
        /// Invalid token
        /// </summary>
        InvalidToken,

        /// <summary>
        /// Refresh token has expired
        /// </summary>
        RefreshTokenExpired,

        /// <summary>
        /// Account has been locked
        /// </summary>
        AccountHasBeenLocked,

        /// <summary>
        /// Access token has not yet expired
        /// </summary>
        AccessTokenNotYetExpired,
    }
}
