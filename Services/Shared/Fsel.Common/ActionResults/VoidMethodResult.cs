using Fsel.Common.Enums.ErrorCodes;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Fsel.Common.ActionResults
{
    public class VoidMethodResult
    {
        private readonly List<ErrorResult> _errorMessages = new List<ErrorResult>();

        public IReadOnlyCollection<ErrorResult> ErrorMessages => _errorMessages;

        public bool IsOK => _errorMessages.Count == 0;

        public int? StatusCode { get; set; }

        private static IList<Error> GetErrors(string? fieldName = null, params object[]? errorValues)
        {
            return new List<Error>() { new Error(fieldName, errorValues) };
        }

        private static IList<Error> GetErrors(params object[]? errorValues)
        {
            return new List<Error>() { new Error(errorValues) };
        }

        public void AddError(ErrorResult? errorResult)
        {
            if (errorResult != null)
            {
                _errorMessages.Add(errorResult);
            }
        }

        public void AddError(IReadOnlyCollection<ErrorResult>? errorResults)
        {
            if (errorResults != null)
            {
                foreach (var errorResult in errorResults)
                {
                    _errorMessages.Add(errorResult);
                }
            }
        }

        public void AddError(string? errorCode, params Error[]? errors)
        {
            if (errors != null)
            {
                _errorMessages.Add(new ErrorResult
                {
                    ErrorCode = errorCode,
                    Errors = errors
                });
            }
        }

        public void AddErrorServer()
        {
            AddError(StatusCodes.Status500InternalServerError, nameof(EnumSystemErrorCode.ServerError));
        }

        public void AddError(string? errorCode)
        {
            _errorMessages.Add(new ErrorResult
            {
                ErrorCode = errorCode,
            });
        }

        public void AddError(string? errorCode, params object[]? errorValues)
        {
            _errorMessages.Add(new ErrorResult
            {
                ErrorCode = errorCode,
                Errors = GetErrors(errorValues)
            });
        }

        public void AddError(string? errorCode, string? fieldName = null, params object[]? errorValues)
        {
            _errorMessages.Add(new ErrorResult
            {
                ErrorCode = errorCode,
                Errors = GetErrors(fieldName, errorValues)
            });
        }

        public void AddError(int statusCode, string? errorCode, string? fieldName, params object[]? errorValues)
        {
            StatusCode = statusCode;
            AddError(errorCode, fieldName, errorValues);
        }

        public void AddError(int statusCode, string? errorCode, string? fieldName, object? errorValue)
        {
            if (errorValue != null)
            {
                StatusCode = statusCode;
                AddError(errorCode, fieldName, errorValue);
            }
        }

        public void AddError(int statusCode, string? errorCode, params Error[]? errors)
        {
            StatusCode = statusCode;
            AddError(errorCode, errors);
        }

        public void AddErrorBadRequest(string? errorCode, string? fieldName = null, params object[]? errorValues)
        {
            AddError(StatusCodes.Status200OK, errorCode, fieldName, errorValues);
        }

        public void AddErrorBadRequest(string? errorCode, string? fieldName, object? errorValue)
        {
            if (errorValue != null)
            {
                AddError(StatusCodes.Status200OK, errorCode, fieldName, errorValue);
            }
        }

        public void AddErrorBadRequest(string? errorCode, params Error[]? errors)
        {
            AddError(StatusCodes.Status200OK, errorCode, errors);
        }

        public virtual IActionResult GetActionResult()
        {
            ObjectResult objectResult = new ObjectResult(this);
            if (!StatusCode.HasValue)
            {
                objectResult.StatusCode = StatusCodes.Status500InternalServerError;
                return objectResult;
            }

            objectResult.StatusCode = StatusCode;
            return objectResult;
        }
    }
}
