using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        /// Sign up fail
        /// </summary>
        AU10ER,
    }
}