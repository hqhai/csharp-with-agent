using Fsel.Common.ActionResults;
using Fsel.Common.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Fsel.Core.Base.Interfaces
{
    public interface IValidationEntity
    {
        Assembly GetAssembly();

        void AddValidationError(string errorCode, string propertyName, object propertyValue);

        void AddValidationError(string errorCode, List<string> errorValues);

        void AddValidationErrors(IEnumerable<ErrorResult> errorMessages);

        bool IsValid();
    }
}
