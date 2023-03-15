// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Enums.ErrorCodes
{
    public enum EnumStudentErrorCode
    {
        /// <summary>
        /// Student does not exist
        /// </summary>
        ST01V,

        /// <summary>
        /// Student have been use
        /// </summary>
        ST02V,

        /// <summary>
        /// Student null
        /// </summary>
        ST03V,

        /// <summary>
        /// Name cannot be empty
        /// </summary>
        ST01C,

        /// <summary>
        /// Name limited to 250 characters
        /// </summary>
        ST02C,

        /// <summary>
        /// Instruction Content limited to 1000 characters
        /// </summary>
        ST03C,

        /// <summary>
        /// Number of course >1
        /// </summary>
        ST04C
    }
}
