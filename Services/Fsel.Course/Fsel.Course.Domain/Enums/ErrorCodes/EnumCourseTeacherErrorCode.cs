// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Enums.ErrorCodes
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public enum EnumCourseTeacherErrorCode
    {
        /// <summary>
        /// Course Teacher does not exist
        /// </summary>
        CT01V,

        /// <summary>
        /// Course Teacher  was used
        /// </summary>
        CT02V,

        /// <summary>
        /// Course Teacher  not is correct
        /// </summary>
        CT03V,

        /// <summary>
        /// Field limited to 1000 characters
        /// </summary>
        CT03C,

        /// <summary>
        /// Course Teacher null
        /// </summary>
        CT04V
    }
}
