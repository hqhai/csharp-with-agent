// Copyright (c) Atlantic. All rights reserved.

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Fsel.Common.ActionResults
{
    public class MethodResult<T> : VoidMethodResult
    {
        public T? Result { get; set; }

        public void AddResultFromErrorList(IEnumerable<ErrorResult>? errorMessages)
        {
            if (errorMessages != null)
            {
                foreach (ErrorResult errorMessage in errorMessages)
                {
                    AddError(errorMessage);
                }
            }
        }

        public override IActionResult GetActionResult()
        {
            ObjectResult objectResult = new ObjectResult(this);
            if (!StatusCode.HasValue)
            {
                if (IsOK)
                {
                    objectResult.StatusCode = ((Result != null) ? StatusCodes.Status200OK : StatusCodes.Status204NoContent);
                }
                else
                {
                    objectResult.StatusCode = StatusCodes.Status500InternalServerError;
                }

                return objectResult;
            }

            objectResult.StatusCode = StatusCode;
            return objectResult;
        }
    }
}
