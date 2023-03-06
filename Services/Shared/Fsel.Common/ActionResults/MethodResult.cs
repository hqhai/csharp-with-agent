using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Fsel.Common.ActionResults
{
    public class MethodResult<T> : VoidMethodResult
    {
        public T? Result { get; set; }

        public void AddResultFromErrorList(IEnumerable<ErrorResult> errorMessages)
        {
            foreach (ErrorResult errorMessage in errorMessages)
            {
                AddErrorMessage(errorMessage);
            }
        }

        public override IActionResult GetActionResult()
        {
            ObjectResult objectResult = new ObjectResult(this);
            if (!base.StatusCode.HasValue)
            {
                if (base.IsOK)
                {
                    objectResult.StatusCode = ((Result != null) ? StatusCodes.Status200OK : StatusCodes.Status204NoContent);
                }
                else
                {
                    objectResult.StatusCode = StatusCodes.Status500InternalServerError;
                }

                return objectResult;
            }

            objectResult.StatusCode = base.StatusCode;
            return objectResult;
        }
    }
}