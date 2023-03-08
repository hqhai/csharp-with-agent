using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fsel.Course.Domain.Enums.ErrorCodes
{
    public enum EnumHomeWorkErrorCode
    {
        /// <summary>
        /// HomeWork does not exist
        /// </summary>
        HW01V,

        HW03V,

        /// <summary>
        /// Name cannot be empty
        /// </summary>
        HW01C,

        /// <summary>
        /// Name limited to 250 characters
        /// </summary>
        HW02C,

        /// <summary>
        /// Instruction Content limited to 1000 characters
        /// </summary>
        HW03C,

        /// <summary>
        /// Number of course >1
        /// </summary>
        HW04C
    }
}