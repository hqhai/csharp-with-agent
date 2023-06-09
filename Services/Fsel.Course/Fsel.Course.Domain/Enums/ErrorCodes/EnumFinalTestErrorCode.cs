// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Enums.ErrorCodes
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public enum EnumFinalTestErrorCode
    {
        /// <summary>
        /// FinalTests does not exist
        /// </summary>
        FinalTestsNotExist,

        /// <summary>
        /// FinalTest  is in active state
        /// </summary>
        FinalTestInActiveState,
    }
}
