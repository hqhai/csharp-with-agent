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
        /// Get List Feature Access Time
        /// </summary>
        [HttpPost("get-list")]
        [ProducesResponseType(typeof(MethodResult<IList<FeatureAccessTimeModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Gets([FromBody] GetFeatureAccessTimesByLessonIdsQuery query)
        {
            var queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Get Feature Access Time By Unit
        /// </summary>
        [HttpPost("get-by-unit")]
        [ProducesResponseType(typeof(MethodResult<IList<FeatureAccessTimeModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetByUnit([FromBody] GetFeatureAccessTimesByUnitIdQuery query)
        {
            var queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Get Feature Access Time By Test
        /// </summary>
        [HttpPost("get-by-test")]
        [ProducesResponseType(typeof(MethodResult<FeatureAccessTimeCourseModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetByTest([FromBody] GetFeatureAccessTimesByTestQuery query)
        {
            var queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Get Feature Access Time By Skill MockTest
        /// </summary>
        [HttpPost("get-by-skill-test")]
        [ProducesResponseType(typeof(MethodResult<FeatureAccessTimeModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetBySkillMockTest([FromBody] GetFeatureAccessTimesByMockTestIdQuery query)
        {
            var queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Get List Feature Access Time By CourseIds
        /// </summary>
        [HttpPost("get-list-by-courseIds")]
        [ProducesResponseType(typeof(MethodResult<IList<FeatureAccessTimeCourseModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Gets([FromBody] GetFeatureAccessTimesByCourseIdsQuery query)
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
