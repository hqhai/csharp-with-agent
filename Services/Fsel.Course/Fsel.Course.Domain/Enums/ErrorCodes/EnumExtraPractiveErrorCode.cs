using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fsel.Course.Domain.Enums.ErrorCodes
{
    public enum EnumExtraPractiveErrorCode
    {
        /// <summary>
        /// ExtraPractive does not exist
        /// </summary>
        EP01V,

        EP03V,

        /// <summary>
        /// Name cannot be empty
        /// </summary>
        EP01C,

        /// <summary>
        /// Name limited to 250 characters
        /// </summary>
        EP02C,

        /// <summary>
        /// Instruction Content limited to 1000 characters
        /// </summary>
        EP03C,

        /// <summary>
        /// Number of course >1
        /// </summary>
        EP04C
    }
}