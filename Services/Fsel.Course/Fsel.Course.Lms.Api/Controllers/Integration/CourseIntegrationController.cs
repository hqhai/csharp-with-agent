// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Api.Controllers.Integration
{
    using System.Net;
    using Asp.Versioning;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Constants;
    using Fsel.Course.Lms.Application.Queries.IntegrationQuery;
    using Fsel.Shared.Constants;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;

    [ApiVersion(ApiSettings.APIVersion1)]
    [ApiVersion(ApiSettings.APIVersion1i1)]
    [Route(Settings.APIDefaultRoute + "/course-integration")]
    [ApiController]
    public class CourseIntegrationController : ControllerBase
    {
        private readonly IMediator _mediator;

        public CourseIntegrationController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Integration Placement Test Result
        /// </summary>
        [HttpGet("integration-placement-test-results")]
        [ProducesResponseType(typeof(MethodResult<IList<object>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetIntegrationPlacementTestResult([FromQuery] IntegrationPlacementTestResultsQuery query)
        {
            var commandResult = await _mediator.Send(query).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Integration Unit
        /// </summary>
        [HttpGet("integration-unit-results")]
        [ProducesResponseType(typeof(MethodResult<IList<object>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetIntegrationUnitResults([FromQuery] IntegrationUnitResultsQuery query)
        {
            var commandResult = await _mediator.Send(query).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }
    }
}
