using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fsel.Course.Domain.Enums.ErrorCodes
{
    public enum EnumLessonUnitErrorCode
    {
        /// <summary>
        /// Lesson Unit does not exist
        /// </summary>
        LU01V,

        /// <summary>
        /// Name cannot be empty
        /// </summary>
        LU01C,

        /// <summary>
        /// Name limited to 250 characters
        /// </summary>
        LU02C,

        /// <summary>
        /// Instruction Content limited to 1000 characters
        /// </summary>
        LU03C,
    }
}
