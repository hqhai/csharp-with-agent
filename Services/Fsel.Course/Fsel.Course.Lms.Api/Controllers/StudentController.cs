// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Api.Controllers
{
    using System.Net;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Constants;
    using Fsel.Common.Enums;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Lms.Application.Commands.CourseCmd;
    using Fsel.Course.Lms.Application.Queries.CourseQuery;
    using MediatR;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;

    [ApiVersion(Settings.APIVersion)]
    [Route(Settings.APIDefaultRoute + "/student")]
    [ApiController]
    [Authorize(Roles = nameof(EnumRole.Student))]
    public class StudentController : ControllerBase
    {
        private readonly IMediator _mediator;

        public StudentController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Get List Course By Level
        /// </summary>
        [HttpGet("courses-by-level")]
        [ProducesResponseType(typeof(MethodResult<IList<CourseModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetListCourseByLevel([FromQuery] GetCoursesByLevelQuery query)
        {
            MethodResult<IList<CourseModel>> queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>z
        /// Get class in course
        /// </summary>
        [HttpGet("class-course")]
        [ProducesResponseType(typeof(MethodResult<string>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetClassIncourse([FromQuery] GetClassCourseQuery query)
        {
            MethodResult<string> queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// create class
        /// </summary>
        [HttpPost("create-class")]
        [ProducesResponseType(typeof(MethodResult<CourseModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Create([FromBody] CreateClassCommand query)
        {
            MethodResult<CourseModel> queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }
    }
}
