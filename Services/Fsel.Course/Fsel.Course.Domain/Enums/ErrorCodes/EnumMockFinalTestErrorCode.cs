using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fsel.Course.Domain.Enums.ErrorCodes
{
    public enum EnumMockFinalTestErrorCode
    {
        /// <summary>
        /// Mock Final Test does not exist
        /// </summary>
        MFT01V,

        /// <summary>
        /// Name cannot be empty
        /// </summary>
        MFT01C,

        /// <summary>
        /// Name limited to 250 characters
        /// </summary>
        MFT02C,

       
    }
}
