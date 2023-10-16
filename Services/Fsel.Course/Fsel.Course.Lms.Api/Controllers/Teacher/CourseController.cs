// Copyright (c) Atlantic. All rights reserved.

using System.Net;
using Fsel.Common.ActionResults;
using Fsel.Common.Constants;
using Fsel.Course.Domain.Models.EntityModels;
using Fsel.Course.Lms.Application.Queries.CourseQuery;
using Fsel.Shared.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Fsel.Course.Lms.Api.Controllers.Teacher
{
    [ApiVersion(Settings.APIVersion)]
    [Route(Settings.APIDefaultRoute + "/teacher/course")]
    [ApiController]
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
        [Authorize(Roles = nameof(EnumRole.Teacher))]
        [HttpGet("get-course-by-level")]
        [ProducesResponseType(typeof(MethodResult<IList<CourseModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetCourseByCourseLevel([FromQuery] GetCoursesByLevelQuery query)
        {
            MethodResult<IList<CourseModel>> commandResult = await _mediator.Send(query).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Get Courses
        /// </summary>
        [HttpGet("get-course-by-ids")]
        [ProducesResponseType(typeof(MethodResult<IList<CourseModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetCourses([FromQuery] IList<Guid> ids)
        {
            var commandResult = await _mediator.Send(new GetCoursesByIdsQuery { CourseIds = ids }).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Get course contain class forum
        /// </summary>
        [Authorize(Roles = nameof(EnumRole.Teacher))]
        [HttpGet("list-courses-contain-class-forum")]
        [ProducesResponseType(typeof(MethodResult<IList<CourseModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetListCourse([FromQuery] GetCoursesContainClassForumQuery query)
        {
            MethodResult<IList<CourseModel>> queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Get course contain mock test
        /// </summary>

        [Authorize(Roles = nameof(EnumRole.Teacher))]
        [HttpGet("list-courses-contain-mock-test")]
        [ProducesResponseType(typeof(MethodResult<IList<CourseModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetListCourseContainMockTest([FromQuery] GetCourseContainMockTestQuery query)
        {
            MethodResult<IList<CourseModel>> queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }
    }
}
