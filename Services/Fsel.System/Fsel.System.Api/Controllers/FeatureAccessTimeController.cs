// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Api.Controllers
{
    using Fsel.Common.ActionResults;
    using Fsel.Common.Constants;
    using Fsel.Shared.Enums;
    using Fsel.System.Application.Commands.FeatureAccessTimeCmd;
    using Fsel.System.Application.Queries.FeatureAccessTimeQuery;
    using Fsel.System.Domain.Models.EntityModels;
    using global::System.Net;
    using MediatR;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;

    [ApiVersion(Settings.APIVersion)]
    [Route(Settings.APIDefaultRoute + "/feature-access-time")]
    [ApiController]
    public class FeatureAccessTimeController : ControllerBase
    {
        private readonly IMediator _mediator;

        public FeatureAccessTimeController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Get Feature Access Time Detail
        /// </summary>
        [HttpGet("get-detail")]
        [ProducesResponseType(typeof(MethodResult<FeatureAccessTimeModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetDetail([FromQuery] GetFeatureAccessTimeQuery query)
        {
            var queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Get Feature Access Times
        /// </summary>
        [HttpPost("gets")]
        [ProducesResponseType(typeof(MethodResult<IList<FeatureAccessTimeModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Gets([FromBody] GetFeatureAccessTimesQuery query)
        {
            var queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Save Feature Access Time
        /// </summary>
        [HttpPost("save")]
        [ProducesResponseType(typeof(MethodResult<FeatureAccessTimeModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [Authorize(Roles = nameof(EnumRole.Student))]
        public async Task<IActionResult> Save([FromBody] SaveFeatureAccessTimeCommand command)
        {
            var queryResult = await _mediator.Send(command).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }
    }
}
