// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Enums.ErrorCodes
{
    public enum EnumAuthErrorCode
    {
        /// <summary>
        /// Old Password cannot empty
        /// </summary>
        AU01V,

        /// <summary>
        /// Password cannot empty
        /// </summary>
        AU02V,

        /// <summary>
        /// Confirm Password cannot empty
        /// </summary>
        AU03V,

        /// <summary>
        /// Email does not exist
        /// </summary>
        AU04V,

        /// <summary>
        /// Old password is incorrect
        /// </summary>
        AU05V,

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
        AU01ER,

        /// <summary>
        /// Error while generating reset token
        /// </summary>
        AU02ER,

        /// <summary>
        /// Error while generating reset Password
        /// </summary>
        AU03ER,

        /// <summary>
        /// Username and Password cannot empty
        /// </summary>
        AU04ER,

        /// <summary>
        /// Username and Password  is incorrect
        /// </summary>
        AU05ER,

        /// <summary>
        /// Invalid token
        /// </summary>
        AU06ER,

        /// <summary>
        /// Access token has not yet expired
        /// </summary>
        AU07ER,

        /// <summary>
        /// Refresh token does not exist
        /// </summary>
        AU08ER,

        /// <summary>
        /// Refresh token has expired
        /// </summary>
        AU09ER,

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
