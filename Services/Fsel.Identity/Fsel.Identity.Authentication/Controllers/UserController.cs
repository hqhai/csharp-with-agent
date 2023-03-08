using Fsel.Common.ActionResults;
using Fsel.Common.Constants;
using Fsel.Common.Helpers;
using Fsel.Identity.Application.Commands.AuthCmd;
using Fsel.Identity.Common.Models.Entities;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace Fsel.Identity.Userentication.Controllers
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
        /// Refresh Token
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        [HttpPost("sign-up")]
        [ProducesResponseType(typeof(MethodResult<UserModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> SignUp([FromBody] SignUpCommand command)
        {
            try
            {
                MethodResult<UserModel> commandResult = await _mediator.Send(command).ConfigureAwait(false);
                return commandResult.GetActionResult();
            }
            catch (Exception ex)
            {
                VoidMethodResult errorCommandResult = new VoidMethodResult();
                errorCommandResult.AddErrorMessage(MethodHelper.GetExceptionMessage(ex));
                return errorCommandResult.GetActionResult();
            }
        }

        /// <summary>
        /// Confirm Email
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        [HttpGet("confirm-email")]
        [ProducesResponseType(typeof(MethodResult<UserModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> ConfirmEmail(string token, string email)
        {
            try
            {
                ConfirmEmailCommand command = new ConfirmEmailCommand
                {
                    Token = token,
                    Email = email
                };
                MethodResult<UserModel> commandResult = await _mediator.Send(command).ConfigureAwait(false);
                return commandResult.GetActionResult();
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