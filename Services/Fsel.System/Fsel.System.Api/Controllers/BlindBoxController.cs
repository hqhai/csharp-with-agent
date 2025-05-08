using System.Net;
using Asp.Versioning;
using Fsel.Common.ActionResults;
using Fsel.Common.Attributes;
using Fsel.Common.Constants;
using Fsel.Shared.Constants;
using Fsel.Shared.Enums;
using Fsel.System.Application.Commands.BlindBoxes;
using Fsel.System.Application.Commands.CourseSuggestConfigCmd;
using Fsel.System.Application.Queries.BlindBoxes;
using Fsel.System.Domain.Models.EntityModels;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Fsel.System.Api.Controllers
{
    [ApiVersion(ApiSettings.APIVersion1)]
    [ApiVersion(ApiSettings.APIVersion1i1)]
    [Route(Settings.APIDefaultRoute + "/blind-box")]
    [ApiController]
    [Permission(role: nameof(EnumRole.Student))]
    public class BlindBoxController : ControllerBase
    {
        private readonly IMediator _mediator;

        public BlindBoxController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Get blind box by student
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(MethodResult<BlindBoxModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetBlindBox()
        {
            var commandResult = await _mediator.Send(new GetBlindBoxQuery()).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Get blind box by student
        /// </summary>
        [HttpGet("get-blind-box-by-user")]
        [ProducesResponseType(typeof(MethodResult<BlindBoxModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetBlindBoxByStudent()
        {
            var commandResult = await _mediator.Send(new GetBlindBoxByUserQuery()).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// buy blind box
        /// </summary>
        [HttpPost("buy-blind-box")]
        [ProducesResponseType(typeof(MethodResult<bool>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> BuyBlindBox([FromBody] BuyBlindBoxConsumerCommand command)
        {
            var commandResult = await _mediator.Send(command).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// get most recent winner
        /// </summary>
        [HttpGet("get-most-recent-winner")]
        [ProducesResponseType(typeof(MethodResult<string?>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetMostRecentWinner()
        {
            var commandResult = await _mediator.Send(new GetMostRecentWinnerQuery()).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// off popup
        /// </summary>
        [HttpPost("off-popup")]
        [ProducesResponseType(typeof(MethodResult<bool>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> OffPopUp()
        {
            var commandResult = await _mediator.Send(new OffPopUpBlindBoxByUserCommand()).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// receive blind box
        /// </summary>
        [HttpPost("receive-blind-box")]
        [ProducesResponseType(typeof(MethodResult<bool>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> ReceiveBlindBox()
        {
            var commandResult = await _mediator.Send(new ReceiveBlindBoxCommand()).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }
    }
}
