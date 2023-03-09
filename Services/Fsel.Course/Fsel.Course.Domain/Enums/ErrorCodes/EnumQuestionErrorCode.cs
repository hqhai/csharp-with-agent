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
        /// Question does not exist
        /// </summary>
        Q01V,

        /// <summary>
        /// Question cannot be empty
        /// </summary>
        Q01C,

        /// <summary>
        /// Config limited to 1000 characters
        /// </summary>
        Q03C,
    }
}