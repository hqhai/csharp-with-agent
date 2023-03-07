using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fsel.Course.Domain.Enums.ErrorCodes
{
    public enum EnumCourseErrorCode
    {
        /// <summary>
        /// Course does not exist
        /// </summary>
        C01V,

        /// <summary>
        /// Name cannot be empty
        /// </summary>
        C01C,

        /// <summary>
        /// Name limited to 250 characters
        /// </summary>
        C02C,

        /// <summary>
        /// Instruction Content limited to 1000 characters
        /// </summary>
        C03C,

        /// <summary>
        /// Number of course >1
        /// </summary>
        C04C,

        /// <summary>
        /// Unit have been use
        /// </summary>
        C02V,
    }
}