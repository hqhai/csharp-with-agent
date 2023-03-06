using Fsel.Common.ActionResults;
using Fsel.Common.Constants;
using Fsel.Common.Helpers;
using Fsel.User.Application.Commands.AuthCmd;
using Fsel.User.Common.Models.Entities;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace Fsel.User.Authentication.Controllers
{
    [ApiVersion(Settings.APIVersion)]
    [Route(Settings.APIDefaultRoute + "/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IMediator _mediator;
        public AuthController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Login
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        [HttpPost("login")]
        [ProducesResponseType(typeof(MethodResult<TokenModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> Login([FromBody] LoginCommand command)
        {
            try
            {
                MethodResult<TokenModel> commandResult = await _mediator.Send(command).ConfigureAwait(false);
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
        /// Refresh Token
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        [HttpPost("refresh-token")]
        [ProducesResponseType(typeof(MethodResult<TokenModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenCommand command)
        {
            try
            {
                MethodResult<TokenModel> commandResult = await _mediator.Send(command).ConfigureAwait(false);
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
        /// Refresh Token
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        [HttpPost("sign-up")]
        [ProducesResponseType(typeof(MethodResult<AccountModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> SignUp([FromBody] SignUpCommand command)
        {
            try
            {
                MethodResult<AccountModel> commandResult = await _mediator.Send(command).ConfigureAwait(false);
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
