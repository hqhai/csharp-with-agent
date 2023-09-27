// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Api.Controllers
{
    using Fsel.Common.ActionResults;
    using Fsel.Common.Constants;
    using Fsel.System.Application.Commands.GameTopicCmd;
    using global::System.Net;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;

    [ApiVersion(Settings.APIVersion)]
    [Route(Settings.APIDefaultRoute + "/game-topic")]
    [ApiController]
    public class GameTopicController : ControllerBase
    {
        private readonly IMediator _mediator;

        public GameTopicController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Save all game topic
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(MethodResult<bool>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Save([FromBody] SaveListGameTopicCommand command)
        {
            var commandResult = await _mediator.Send(command).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }
    }
}
