// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Api.Controllers.Integration
{
    using System.Net;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Constants;
    using Fsel.Course.Domain.Models.EntityModels.IntegrationModels;
    using Fsel.Course.Lms.Application.Queries.IntegrationQuery;
    using Fsel.Shared.Attributes;
    using Fsel.Shared.Constants;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;

    [ApiVersions(ApiSettings.APIVersion1)]
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
        [HttpPost("integration-placement-test-results")]
        [ProducesResponseType(typeof(MethodResult<IList<object>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetIntegrationPlacementTestResult([FromBody] IntegrationPlacementTestResultsQuery query)
        {
            var commandResult = await _mediator.Send(query).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Integration Unit
        /// </summary>
        [HttpPost("integration-unit-results")]
        [ProducesResponseType(typeof(MethodResult<IList<object>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetIntegrationUnitResults([FromBody] IntegrationUnitResultsQuery query)
        {
            var commandResult = await _mediator.Send(query).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Integration Info Course
        /// </summary>
        [HttpPost("info-course-integration")]
        [ProducesResponseType(typeof(MethodResult<IList<InfoCourseIntegrationModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetInfoCourseIntegration([FromBody] GetInfoCourseIntegrationQuery query)
        {
            var commandResult = await _mediator.Send(query).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }
    }
}
