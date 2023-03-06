using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fsel.Common.ActionResults
{
    public class VoidMethodResult
    {
        private readonly List<ErrorResult> _errorMessages = new List<ErrorResult>();

        public IReadOnlyCollection<ErrorResult> ErrorMessages => _errorMessages;

        public bool IsOK => _errorMessages.Count == 0;

        public int? StatusCode { get; set; }

        public void AddErrorMessage(ErrorResult errorResult)
        {
            _errorMessages.Add(errorResult);
        }

        public void AddErrorMessage(string errorCode, string errorMessage, string[] errorValues)
        {
            ErrorResult errorResult = new ErrorResult
            {
                ErrorCode = errorCode,
                ErrorMessage = errorMessage
            };
            if (errorValues != null && errorValues.Length != 0)
            {
                foreach (string item in errorValues)
                {
                    errorResult.ErrorValues.Add(item);
                }
            }

            AddErrorMessage(errorResult);
        }

        public void AddErrorMessage(string exceptionErrorMessage)
        {
            AddErrorMessage("ERR_COM_API_SERVER_ERROR", new string[0], exceptionErrorMessage);
        }

        private void AddErrorMessage(string errorCode, string[] errorValues, string exceptionErrorMessage)
        {
            _errorMessages.Add(new ErrorResult
            {
                ErrorCode = errorCode,
                ErrorMessage = "Error: " + exceptionErrorMessage,
                ErrorValues = new List<string>(errorValues)
            });
        }

        private void AddErrorMessage(string errorCode, string errorMessage, string[] errorValues, string exceptionErrorMessage)
        {
            _errorMessages.Add(new ErrorResult
            {
                ErrorCode = errorCode,
                ErrorMessage = "Error: " + errorMessage + ", Exception Message: " + exceptionErrorMessage,
                ErrorValues = new List<string>(errorValues)
            });
        }

        public virtual IActionResult GetActionResult()
        {
            ObjectResult objectResult = new ObjectResult(this);
            if (!StatusCode.HasValue)
            {
                objectResult.StatusCode = 500;
                return objectResult;
            }

            objectResult.StatusCode = StatusCode;
            return objectResult;
        }
    }
}
