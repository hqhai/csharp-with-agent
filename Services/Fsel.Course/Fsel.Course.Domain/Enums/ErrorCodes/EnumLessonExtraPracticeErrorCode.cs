using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fsel.Course.Domain.Enums.ErrorCodes
{
    public enum EnumLessonExtraPracticeErrorCode
    {
        /// <summary>
        /// Lesson ExtraPractive does not exist
        /// </summary>
        ELP01V,

        /// <summary>
        /// Lesson ExtraPractive have been use
        /// </summary>
        ELP02V,

        /// <summary>
        /// Lesson ExtraPractive null
        /// </summary>
        ELP03V,

        /// <summary>
        /// Name cannot be empty
        /// </summary>
        ELP01C,

        /// <summary>
        /// Name limited to 250 characters
        /// </summary>
        ELP02C,

        /// <summary>
        /// Instruction Content limited to 1000 characters
        /// </summary>
        ELP03C,

        /// <summary>
        /// Number of Lesson ExtraPractive >1
        /// </summary>
        ELP04C
    }
}