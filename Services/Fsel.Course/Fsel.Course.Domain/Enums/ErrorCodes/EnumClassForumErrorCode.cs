using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fsel.Course.Domain.Enums.ErrorCodes
{
    public enum EnumClassForumErrorCode
    {
        /// <summary>
        /// ClassForum does not exist
        /// </summary>
        CF01V,

        /// <summary>
        /// Name cannot be empty
        /// </summary>
        CF01C,

        /// <summary>
        /// Name limited to 250 characters
        /// </summary>
        CF02C,

        /// <summary>
        /// Instruction Content limited to 1000 characters
        /// </summary>
        CF03C,

        /// <summary>
        /// Number of course >1
        /// </summary>
        CF04C
    }
}