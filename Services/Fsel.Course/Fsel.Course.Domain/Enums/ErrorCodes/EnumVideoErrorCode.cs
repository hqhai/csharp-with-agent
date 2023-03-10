using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fsel.Course.Domain.Enums.ErrorCodes
{
    public enum EnumVideoErrorCode
    {
        /// <summary>
        /// Video does not exist
        /// </summary>
        VD01V,

        /// <summary>
        /// Video was used
        /// </summary>
        VD02V,

        /// <summary>
        /// Video not is correct
        /// </summary>
        VD03V,

        /// <summary>
        /// Name cannot be empty
        /// </summary>
        VD01C,

        /// <summary>
        /// Name limited to 250 characters
        /// </summary>
        VD02C,

        /// <summary>
        /// Instruction Content limited to 1000 characters
        /// </summary>
        VD03C,
    }
}