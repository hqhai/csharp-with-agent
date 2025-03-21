using System.Net;
using Asp.Versioning;
using Fsel.Common.ActionResults;
using Fsel.Common.Attributes;
using Fsel.Common.Constants;
using Fsel.Shared.Constants;
using Fsel.Shared.Enums;
using Fsel.System.Application.Commands.BlindBoxes;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Fsel.System.Api.Controllers.Admins
{
    [ApiVersion(ApiSettings.APIVersion1)]
    [ApiVersion(ApiSettings.APIVersion1i1)]
    [Route(Settings.APIDefaultRoute + "/admin/blind-box")]
    [ApiController]
    [Permission(role: nameof(EnumRole.Admin))]
    public class BlindBoxController : ControllerBase
    {
        private readonly IMediator _mediator;

        public BlindBoxController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// add user into blind box
        /// </summary>
        [HttpPost("add-user-into-blind-box")]
        [ProducesResponseType(typeof(MethodResult<bool>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> AddUserIntoBlindBox([FromBody] AddUserIntoBlindBoxCommand command)
        {
            var commandResult = await _mediator.Send(command).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }
    }
}
