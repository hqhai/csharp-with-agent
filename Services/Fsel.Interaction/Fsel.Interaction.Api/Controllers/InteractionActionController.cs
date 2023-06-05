// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Api.Controllers
{
    using Fsel.Common.ActionResults;
    using System.Net;
    using Fsel.Common.Constants;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;
    using Fsel.Interaction.Application.Commands.ActionCmd;

    [ApiVersion(Settings.APIVersion)]
    [Route(Settings.APIDefaultRoute + "/interaction-action")]
    [ApiController]
    public class InteractionActionController : ControllerBase
    {
        private readonly IMediator _mediator;

        public InteractionActionController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Create action
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(MethodResult<bool>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Create([FromBody] CreateActionCommand command)
        {
            MethodResult<bool> queryResult = await _mediator.Send(command).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }
    }
}
