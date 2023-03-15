// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Enums.ErrorCodes
{
    public enum EnumHumanErrorCode
    {
        /// <summary>
        /// Human does not exist
        /// </summary>
        HM01V,

        /// <summary>
        /// Human have been use
        /// </summary>
        HM02V,

        /// <summary>
        /// Human null
        /// </summary>
        HM03V,

        /// <summary>
        /// Name cannot be empty
        /// </summary>
        HM01C,

        /// <summary>
        /// Name limited to 250 characters
        /// </summary>
        HM02C,

        /// <summary>
        /// Instruction Content limited to 1000 characters
        /// </summary>
        HM03C,

        /// <summary>
        /// Number of course >1
        /// </summary>
        HM04C
    }
}
