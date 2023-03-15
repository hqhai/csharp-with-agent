// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Enums.ErrorCodes
{
    public enum EnumParentStudentErrorCode
    {
        /// <summary>
        /// ParentStudent does not exist
        /// </summary>
        PS01V,

        /// <summary>
        /// ParentStudent have been use
        /// </summary>
        PS02V,

        /// <summary>
        /// ParentStudent null
        /// </summary>
        PS03V,

        /// <summary>
        /// Name cannot be empty
        /// </summary>
        PS01C,

        /// <summary>
        /// Number of course >1
        /// </summary>
        PS04C
    }
}
