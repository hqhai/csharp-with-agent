using Fsel.Common.ActionResults;
using System.Reflection;

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