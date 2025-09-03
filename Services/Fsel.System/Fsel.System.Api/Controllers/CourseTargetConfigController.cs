// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Api.Controllers
{
    using Asp.Versioning;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Attributes;
    using Fsel.Common.Constants;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Models.ShareModels;
    using Fsel.System.Application.Commands.CourseTargetConfigCmd;
    using Fsel.System.Application.Queries.CourseTargetConfigQuery;
    using global::System.Net;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;

    [ApiVersion(ApiSettings.APIVersion1)]
    [ApiVersion(ApiSettings.APIVersion1i1)]
    [Route(Settings.APIDefaultRoute + "/course-target-config")]
    [ApiController]
    public class CourseTargetConfigController : ControllerBase
    {
        private readonly IMediator _mediator;

        public CourseTargetConfigController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// create-course-target-config
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(MethodResult<CourseTargetConfigModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [Permission(CourseGoalManagement.Add)]
        public async Task<IActionResult> CreateCourseTargetConfig([FromBody] CreateCourseTargetConfigCommand command)
        {
            var commandResult = await _mediator.Send(command).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// update-course-target-config
        /// </summary>
        [HttpPut]
        [ProducesResponseType(typeof(MethodResult<CourseTargetConfigModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [Permission(CourseGoalManagement.Update)]
        public async Task<IActionResult> UpdateCourseTargetConfig([FromBody] UpdateCourseTargetConfigCommand command)
        {
            var commandResult = await _mediator.Send(command).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// delete-course-target-config
        /// </summary>
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(MethodResult<CourseTargetConfigModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [Permission(CourseGoalManagement.Update)]
        public async Task<IActionResult> DeleteCourseTargetConfig([FromRoute] Guid id)
        {
            var commandResult = await _mediator.Send(new DeleteCourseTargetConfigCommand { Id = id }).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// get-course-target-configs
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(MethodResult<IList<CourseTargetConfigModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [Permission(CourseGoalManagement.View)]
        public async Task<IActionResult> GetCourseTargetConfig([FromQuery] GetCourseTargetConfigQuery query)
        {
            var queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// get-course-target-configs-by-course-level
        /// </summary>
        [HttpGet("course-target-course-level")]
        [ProducesResponseType(typeof(MethodResult<IList<CourseTargetConfigModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [Permission(CourseGoalManagement.View)]
        public async Task<IActionResult> GetCourseTargetConfigByCourseLevel([FromQuery] GetCourseTargetConfigByCourseLevelQuery query)
        {
            var queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }
    }
}
