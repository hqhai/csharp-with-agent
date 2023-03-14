// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Enums.ErrorCodes
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

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
        PA04C
    }
}
