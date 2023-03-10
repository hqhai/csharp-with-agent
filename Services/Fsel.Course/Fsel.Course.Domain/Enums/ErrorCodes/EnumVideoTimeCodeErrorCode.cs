using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fsel.Course.Domain.Enums.ErrorCodes
{
    public enum EnumVideoTimeCodeErrorCode
    {
        /// <summary>
        /// Number of DisplayTime >1 ,Number of ExecutionTime >1
        /// </summary>
        VTC04C,

        /// <summary>
        /// Video Time Code not is correct
        /// </summary>
        VTC03V,
    }
}