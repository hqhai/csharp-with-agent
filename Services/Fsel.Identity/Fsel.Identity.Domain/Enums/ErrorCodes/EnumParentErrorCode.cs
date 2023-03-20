// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Enums.ErrorCodes
{
    public enum EnumParentErrorCode
    {
        /// <summary>
        /// Parent does not exist
        /// </summary>
        PA01V,

        /// <summary>
        /// Parent have been use
        /// </summary>
        PA02V,

        /// <summary>
        /// Parent null
        /// </summary>
        PA03V,

        /// <summary>
        /// Account Student by Parent has been initialized error
        /// </summary>
        PA04V,

        /// <summary>
        /// Send Student Account Email Failed
        /// </summary>
        PA05V,

        /// <summary>
        /// TransactionS error
        /// </summary>
        PA06V,

        /// <summary>
        /// parent had more than 2 students
        /// </summary>
        PA07V,

        /// <summary>
        /// Request not null
        /// </summary>
        PA08V,

        /// <summary>
        /// Name cannot be empty
        /// </summary>
        PA01C,

        /// <summary>
        /// Name limited to 250 characters
        /// </summary>
        PA02C,

        /// <summary>
        /// Instruction Content limited to 1000 characters
        /// </summary>
        PA03C,

        /// <summary>
        /// Number of course >1
        /// </summary>
        PA04C,

        /// <summary>
        /// Username already exists
        /// </summary>
        PA05C
    }
}
