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
        public async Task<IActionResult> Gets([FromBody] IList<Guid> ids, [FromQuery] Guid userId, [FromQuery] Guid courseId, [FromQuery] Guid? unitId, [FromQuery] Guid? lessonId)
        {
            var queryResult = await _mediator.Send(new GetFeatureAccessTimesByIdsQuery { Ids = ids, UserId = userId, CourseId = courseId, UnitId = unitId, LessonId = lessonId }).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Get List Feature Access Time By CourseIds
        /// </summary>
        [HttpPost("get-list-by-courseIds")]
        [ProducesResponseType(typeof(MethodResult<IList<FeatureAccessTimeCourseModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Gets([FromBody] IList<Guid> courseIds, [FromQuery] Guid userId)
        {
            var queryResult = await _mediator.Send(new GetFeatureAccessTimesByCourseIdsQuery { CourseIds = courseIds, UserId = userId }).ConfigureAwait(false);
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
