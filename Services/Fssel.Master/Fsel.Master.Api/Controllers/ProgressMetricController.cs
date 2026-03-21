// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Master.Api.Controllers
{
    using System.Net;
    using Asp.Versioning;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Constants;
    using Fsel.Master.Application.Queries.ProgressMetrics;
    using Fsel.Master.Domain.Models.EntityModels;
    using Fsel.Shared.Constants;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;

    [ApiVersion(ApiSettings.APIVersion1)]
    [ApiVersion(ApiSettings.APIVersion1i1)]
    [Route(Settings.APIDefaultRoute + "/progress-metric")]
    [ApiController]
    public class ProgressMetricController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ProgressMetricController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("get-placement-test-overview")]
        [ProducesResponseType(typeof(MethodResult<PlacementTestDashboardModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetPlacementTestOverview([FromQuery] GetPlacementTestOverviewQuery query)
        {
            var queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        [HttpGet("get-province-level-distribution")]
        [ProducesResponseType(typeof(MethodResult<ProvinceLevelResponse>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetProvinceLevelDistribution([FromQuery] GetProvinceLevelDistributionQuery query)
        {
            var queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }
    }
}
