// Copyright (c) Atlantic. All rights reserved.

using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Text.Json.Serialization;
using Fsel.Common.ActionResults;

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

        public IList<Error> GetErrors(IList<object>? errorValues, string? fieldName = null, IList<object>? exactValues = null)
        {
            return new List<Error>() { new Error(fieldName, errorValues, exactValues) };
        }

        public ErrorResult GetErrorResult(string? errorCode, IList<object>? errorValues, string? fieldName = null, IList<object>? exactValues = null)
        {
            return new ErrorResult
            {
                ErrorCode = errorCode,
                Errors = GetErrors(errorValues, fieldName, exactValues)
            };
        }

        public void AddErrorResults(string? errorCode, IList<object>? errorValues, string? fieldName = null, IList<object>? exactValues = null)
        {
            _errorMessages.Add(GetErrorResult(errorCode, errorValues, fieldName, exactValues));
        }

        public void AddErrorResults(string? errorCode, IList<Error> errors)
        {
            _errorMessages.Add(new ErrorResult
            {
                ErrorCode = errorCode,
                Errors = errors
            });
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

                        if (value != null)
                        {
                            var errorValues = new List<object> { value };
                            var customAttrs = property?.GetCustomAttributesData();
                            var customAttr = customAttrs?.FirstOrDefault(x => x.NamedArguments.Select(n => n.TypedValue.Value).Contains(item.ErrorMessage));
                            var extracValues = customAttr?.ConstructorArguments.Select(x => x.Value).Cast<object>().ToList();

                            errorResult.Errors.Add(new Error(memberName, errorValues, extracValues));
                        }
                        else
                        {
                            errorResult.Errors.Add(new Error(memberName));
                        }
                    }
                    AddErrorResults(errorResult);
                }
            }

            return _errorMessages.Count == 0;
        }

        #endregion Validation
    }
}
