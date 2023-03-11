using Fsel.Common.ActionResults;
using Fsel.Common.Helpers;
using Fsel.Core.Base.Interfaces;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Text.Json.Serialization;

namespace Fsel.Core.Base
{
    public class ValidationEntity : IValidationEntity
    {
        #region Validation

        protected List<ErrorResult> _errorMessages = new List<ErrorResult>();

        [JsonIgnore]
        public IReadOnlyCollection<ErrorResult> ErrorMessages => _errorMessages;

        public Assembly GetAssembly()
        {
            return GetType().Assembly;
        }

        public void AddValidationError(string errorCode, string propertyName, object propertyValue)
        {
            AddValidationError(errorCode, new List<string> { MethodHelper.GenerateErrorResult(propertyName, propertyValue) });
        }

        public void AddValidationError(string errorCode, List<string> errorValues)
        {
            _errorMessages.Add(new ErrorResult
            {
                ErrorCode = errorCode,
                ErrorMessage = MethodHelper.GetErrorMessage(errorCode, GetAssembly()),
                ErrorValues = errorValues
            });
        }

        public void AddValidationErrors(IEnumerable<ErrorResult> errorMessages)
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
                    errorResult.ErrorMessage = MethodHelper.GetErrorMessage(item.ErrorMessage, GetAssembly());
                    foreach (string memberName in item.MemberNames)
                    {
                        PropertyInfo? property = validationContext.ObjectType.GetProperty(memberName);
                        object? value = property?.GetValue(validationContext.ObjectInstance, null);
                        errorResult.ErrorValues.Add(MethodHelper.GenerateErrorResult(memberName, value));
                    }

                    _errorMessages.Add(errorResult);
                }
            }

            return _errorMessages.Count == 0;
        }

        #endregion Validation
    }
}