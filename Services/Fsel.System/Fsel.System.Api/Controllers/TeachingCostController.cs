// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Api.Controllers
{
    using Fsel.Common.ActionResults;
    using Fsel.Common.Constants;
    using Fsel.System.Application.Commands.TeachingCostCmd;
    using Fsel.System.Application.Queries.TeachingCostQuery;
    using Fsel.System.Domain.Models.EntityModels;
    using global::System.Net;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;

    [ApiVersion(Settings.APIVersion)]
    [Route(Settings.APIDefaultRoute + "/teaching-cost")]
    [ApiController]
    public class TeachingCostController : ControllerBase
    {
        private readonly IMediator _mediator;

        public TeachingCostController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Get Teaching Cost by level
        /// </summary>
        [HttpGet("get-by-level")]
        [ProducesResponseType(typeof(MethodResult<TeachingCostModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetTeachingCostByLevel([FromQuery] GetListTeachingCostByCourseLevel query)
        {
            MethodResult<TeachingCostModel> commandResult = await _mediator.Send(query).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Save teaching cost
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(MethodResult<TeachingCostModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Save([FromBody] SaveTeachingCostCommand command)
        {
            MethodResult<TeachingCostModel> commandResult = await _mediator.Send(command).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }
    }
}
