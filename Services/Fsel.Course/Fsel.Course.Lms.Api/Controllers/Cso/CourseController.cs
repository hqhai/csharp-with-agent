// Copyright (c) Atlantic. All rights reserved.

using System.Net;
using Fsel.Common.ActionResults;
using Fsel.Common.Attributes;
using Fsel.Common.Constants;
using Fsel.Course.Domain.Models.EntityModels;
using Fsel.Course.Lms.Application.Commands.CourseResultCmd.AdminCmd;
using Fsel.Course.Lms.Application.Queries.CourseQuery;
using Fsel.Shared.Attributes;
using Fsel.Shared.Constants;
using Fsel.Shared.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Fsel.Course.Lms.Api.Controllers.Cso
{
    [ApiVersions(ApiSettings.APIVersion1)]
    [Route(Settings.APIDefaultRoute + "/cso/course")]
    [ApiController]
    [Permission]
    public class CourseController : ControllerBase
    {
        private readonly IMediator _mediator;

        public CourseController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Get Course
        /// </summary>
        [HttpGet("get-course-by-level")]
        [ProducesResponseType(typeof(MethodResult<IList<CourseModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [Permission(roles: new string[] { nameof(EnumRole.Admin), nameof(EnumRole.AdminSchool), nameof(EnumRole.CSO) })]
        public async Task<IActionResult> GetCourseByCourseLevel([FromQuery] GetCoursesByLevelQuery query)
        {
            MethodResult<IList<CourseModel>> commandResult = await _mediator.Send(query).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Get Course level
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(MethodResult<CourseModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [Permission(roles: new string[] { nameof(EnumRole.Admin), nameof(EnumRole.AdminSchool), nameof(EnumRole.CSO) })]
        public async Task<IActionResult> GetCourseLevel([FromRoute] Guid id)
        {
            MethodResult<CourseModel> commandResult = await _mediator.Send(new GetCourseLevelQuery { CourseId = id }).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Active Manage Courses
        /// </summary>
        [HttpPost("change-course-level")]
        [ProducesResponseType(typeof(MethodResult<IList<LevelDtoModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [Permission(StudentManagement.Update)]
        public async Task<IActionResult> ChangeCourseLevel([FromBody] ChangeCourseLevelByAdminCommand command)
        {
            var commandResult = await _mediator.Send(command).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Retake Manage Courses
        /// </summary>
        [HttpPost("retake-course")]
        [ProducesResponseType(typeof(MethodResult<CourseResultModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [Permission(StudentManagement.Update)]
        public async Task<IActionResult> RetakeCourse([FromBody] RetakeCourseResultByAdminCommand command)
        {
            var commandResult = await _mediator.Send(command).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }
    }
}
