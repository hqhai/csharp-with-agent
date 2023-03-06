using Fsel.Common.ActionResults;
using Fsel.Common.Constants;
using Fsel.Common.Helpers;
using Fsel.User.Application.Commands.AuthCmd;
using Fsel.User.Common.Models.Entities;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace Fsel.User.Api.Controllers
{
    [ApiVersion(Settings.APIVersion)]
    [Route(Settings.APIDefaultRoute + "/user")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IMediator _mediator;
        public UserController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Login
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        [Authorize]
        [HttpGet]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.BadRequest)]
        public IActionResult Get()
        {
            try
            {
                return Ok(new string[] { "Catcher Wong", "James Li" });
            }
            catch (Exception ex)
            {
                VoidMethodResult errorCommandResult = new VoidMethodResult();
                errorCommandResult.AddErrorMessage(MethodHelper.GetExceptionMessage(ex));
                return errorCommandResult.GetActionResult();
            }
        }
    }
}
