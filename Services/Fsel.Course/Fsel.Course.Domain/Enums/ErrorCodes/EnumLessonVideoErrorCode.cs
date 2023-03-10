using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fsel.Course.Domain.Enums.ErrorCodes
{
    public enum EnumLessonVideoErrorCode
    {
        /// <summary>
        /// Lesson Video does not exist
        /// </summary>
        LV01V,

        /// <summary>
        /// Lesson Video was used
        /// </summary>
        LV02V,

        /// <summary>
        /// Lesson Video not is correct
        /// </summary>
        LV03V,

        /// <summary>
        /// Name cannot be empty
        /// </summary>
        LV01C,

        /// <summary>
        /// Name limited to 250 characters
        /// </summary>
        LV02C,

        /// <summary>
        /// Instruction Content limited to 1000 characters
        /// </summary>
        LV03C,
    }
}