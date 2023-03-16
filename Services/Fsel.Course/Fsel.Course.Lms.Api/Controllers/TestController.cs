using System.Net;
using Fsel.Common.ActionResults;
using Fsel.Common.Constants;
using Fsel.Common.Enums;
using Fsel.Common.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Fsel.Course.Lms.Api.Controllers
{
    [ApiVersion(Settings.APIVersion)]
    [Route(Settings.APIDefaultRoute + "/test")]
    [ApiController]
    [Authorize(Roles = nameof(EnumRole.Parent))]
    [Authorize(Roles = nameof(EnumRole.Student))]
    public class TestController : ControllerBase
    {
        public TestController()
        {
        }

        /// <summary>
        /// Search Course
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(MethodResult<string>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.BadRequest)]
        public IActionResult Search()
        {
            try
            {
                MethodResult<string> queryResult = new MethodResult<string> { Result = nameof(Search) };
                return queryResult.GetActionResult();
            }
            catch (Exception ex)
            {
                VoidMethodResult errorResult = new VoidMethodResult();
                errorResult.AddErrorMessage(MethodHelper.GetExceptionMessage(ex));
                return errorResult.GetActionResult();
            }
        }
    }
}
