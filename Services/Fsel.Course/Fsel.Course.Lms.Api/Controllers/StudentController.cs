// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Api.Controllers
{
    using System.Net;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Constants;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Lms.Application.Queries.CourseQuery;
    using Fsel.Course.Lms.Application.Queries.Students;
    using Fsel.Shared.Enums;
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
        /// Get List Course Teacher
        /// </summary>
        [HttpGet("get-list-course-teacher")]
        [ProducesResponseType(typeof(MethodResult<IList<CourseModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetListCourseTeacher([FromQuery] GetListCourseTeacherQuery query)
        {
            MethodResult<IList<CourseModel>> queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        ///// <summary>z
        ///// Get code class
        ///// </summary>
        //[HttpGet("get-class-code")]
        //[ProducesResponseType(typeof(MethodResult<string>), (int)HttpStatusCode.OK)]
        //[ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        //public async Task<IActionResult> GetClassIncourse([FromQuery] get query)
        //{
        //    MethodResult<string> queryResult = await _mediator.Send(query).ConfigureAwait(false);
        //    return queryResult.GetActionResult();
        //}

        /// <summary>
        /// Setting Student
        /// </summary>
        [HttpGet("setting-student")]
        [ProducesResponseType(typeof(MethodResult<StudentSettingModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> SettingStudent()
        {
            MethodResult<StudentSettingModel> queryResult = await _mediator.Send(new GetStudentSettingQuery()).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }
    }
}
