// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Api.Controllers
{
    using Asp.Versioning;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Constants;
    using Fsel.Shared.Constants;
    using Fsel.System.Application.Commands.DisplayOrderConfigCmd;
    using Fsel.System.Application.Queries.DisplayOrderConfigQuery;
    using Fsel.System.Domain.Models.EntityModels;
    using global::System.Net;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;

    [ApiVersion(ApiSettings.APIVersion1)]
    [ApiVersion(ApiSettings.APIVersion1i1)]
    [Route(Settings.APIDefaultRoute + "/display-order-config")]
    [ApiController]
    public class DisplayOrderConfigController : ControllerBase
    {
        private readonly IMediator _mediator;

        public DisplayOrderConfigController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Get Display Order Config
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(MethodResult<IList<DisplayOrderConfigModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Get()
        {
            var commandResult = await _mediator.Send(new GetDisplayOrderConfigQuery()).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Update Display Order Config
        /// </summary>
        [HttpPut]
        [ProducesResponseType(typeof(MethodResult<IList<DisplayOrderConfigModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Update([FromBody] UpdateDisplayOrderConfigCommand command)
        {
            var commandResult = await _mediator.Send(command).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }
    }
}
