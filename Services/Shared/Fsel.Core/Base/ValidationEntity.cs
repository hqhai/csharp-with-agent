using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Text.Json.Serialization;
using Fsel.Common.ActionResults;
using Fsel.Common.Helpers;

namespace Fsel.Core.Base
{
    public class ValidationEntity
    {
        #region Validation

        protected List<ErrorResult> _errorMessages = new List<ErrorResult>();

        [JsonIgnore]
        public IReadOnlyCollection<ErrorResult> ErrorMessages => _errorMessages;

        public Assembly GetAssembly()
        {
            return GetType().Assembly;
        }

        public Error AddError(IList<object> errorValues, string? errorField = null)
        {
            return new Error
            {
                ErrorField = errorField,
                ErrorValues = errorValues
            };
        }

        public IList<Error> AddErrors(IList<object> errorValues, string? errorField = null)
        {
            return new List<Error>() { AddError(errorValues, errorField) };
        }

        public ErrorResult AddErrorResult(string errorCode, IList<object> errorValues, string? errorField = null)
        {
            return new ErrorResult
            {
                ErrorCode = errorCode,
                Errors = AddErrors(errorValues, errorField)
            };
        }

        public void AddErrorResults(string errorCode, IList<object> errorValues, string? errorField = null)
        {
            _errorMessages.Add(AddErrorResult(errorCode, errorValues, errorField));
        }

        public void AddErrorResults(ErrorResult errorMessages)
        {
            _errorMessages.Add(errorMessages);
        }

        public void AddErrorResults(IEnumerable<ErrorResult> errorMessages)
        {
            _errorMessages.AddRange(errorMessages);
        }

        public virtual bool IsValid()
        {
            ValidationContext validationContext = new ValidationContext(this, null, null);
            List<ValidationResult> list = new List<ValidationResult>();
            if (!Validator.TryValidateObject(this, validationContext, list, validateAllProperties: true))
            {
                foreach (ValidationResult item in list)
                {
                    ErrorResult errorResult = new ErrorResult
                    {
                        ErrorCode = item.ErrorMessage
                    };

                    //errorResult.ErrorMessage = MethodHelper.GetErrorMessage(item.ErrorMessage, GetAssembly());
                    foreach (string memberName in item.MemberNames)
                    {
                        PropertyInfo? property = validationContext.ObjectType.GetProperty(memberName);
                        object? value = property?.GetValue(validationContext.ObjectInstance, null);
                        //errorResult.ErrorValues.Add(MethodHelper.GenerateErrorResult(memberName, value));
                    }

                    _errorMessages.Add(errorResult);
                }
            }

            return _errorMessages.Count == 0;
        }

        #endregion Validation
    }
}
