// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Api.Controllers
{
    using System.Net;
    using Asp.Versioning;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Constants;
    using Fsel.Interaction.Application.Queries.HarmfulContentQuery;
    using Fsel.Shared.Constants;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;

    [ApiVersion(ApiSettings.APIVersion1)]
    [ApiVersion(ApiSettings.APIVersion1i1)]
    [Route(Settings.APIDefaultRoute + "/harmful-content")]
    [ApiController]
    public class HarmfulContentController : ControllerBase
    {
        private readonly IMediator _mediator;

        public HarmfulContentController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Check harmful content words
        /// </summary>
        [HttpPost("harmful-content-words")]
        [ProducesResponseType(typeof(MethodResult<bool>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> HarmfulContentWords([FromQuery] CheckHarmfulContentWordsQuery query)
        {
            var queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Check harmful content image
        /// </summary>
        [HttpPost("harmful-content-image")]
        [ProducesResponseType(typeof(MethodResult<bool>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> HarmfulContentImage([FromQuery] CheckHarmfulContentImageQuery query)
        {
            var queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }
    }
}
