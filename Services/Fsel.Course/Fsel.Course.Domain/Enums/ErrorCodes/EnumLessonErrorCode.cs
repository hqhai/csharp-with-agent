using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fsel.Course.Domain.Enums.ErrorCodes
{
    public enum EnumLessonErrorCode
    {
        /// <summary>
        /// Lesson does not exist
        /// </summary>
        LS01V,

        /// <summary>
        /// Name cannot be empty
        /// </summary>
        LS01C,

        /// <summary>
        /// Name limited to 250 characters
        /// </summary>
        LS02C,

        /// <summary>
        /// Instruction Content limited to 1000 characters
        /// </summary>
        LS03C,
    }
}
