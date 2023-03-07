using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fsel.Course.Domain.Enums.ErrorCodes
{
    public enum EnumUnitErrorCode
    {
        /// <summary>
        /// Unit does not exist
        /// </summary>
        U01V,
        /// <summary>
        /// Name cannot be empty
        /// </summary>
        U01C,

        /// <summary>
        /// Name limited to 250 characters
        /// </summary>
        U02C,

        /// <summary>
        /// Instruction Content limited to 1000 characters
        /// </summary>
        U03C,
    }
}
