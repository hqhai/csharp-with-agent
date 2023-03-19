using System.Net;
using Fsel.Common.ActionResults;
using Fsel.Common.Constants;
using Fsel.Common.Helpers;
using Fsel.Identity.Application.Commands.AuthCmd;
using Fsel.Identity.Domain.Models.EntityModels;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Fsel.Identity.Authentication.Controllers
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
            catch
            {
                VoidMethodResult errorCommandResult = new VoidMethodResult();
                errorCommandResult.AddError();
                return errorCommandResult.GetActionResult();
            }
        }

        /// <summary>
        /// Refresh Token
        /// </summary>
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
            catch
            {
                VoidMethodResult errorResult = new VoidMethodResult();
                errorResult.AddError();
                return errorResult.GetActionResult();
            }
        }
    }
}
