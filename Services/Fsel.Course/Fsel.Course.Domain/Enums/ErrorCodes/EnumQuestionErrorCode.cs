using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fsel.Course.Domain.Enums.ErrorCodes
{
    public enum EnumQuestionErrorCode
    {
        /// <summary>
        /// Course does not exist
        /// </summary>
        Q01V,

        /// <summary>
        /// Name cannot be empty
        /// </summary>
        Q01C,

        /// <summary>
        /// Name limited to 250 characters
        /// </summary>
        Q02C,

        /// <summary>
        /// Instruction Content limited to 1000 characters
        /// </summary>
        Q03C,
    }
}