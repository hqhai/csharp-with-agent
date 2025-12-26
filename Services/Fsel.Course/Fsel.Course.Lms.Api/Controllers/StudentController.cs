// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Api.Controllers
{
    using System.Net;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Constants;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Lms.Application.Queries.CourseQuery;
    using Fsel.Course.Lms.Application.Queries.StudentQuery;
    using Fsel.Shared.Attributes;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;

    [ApiVersions(ApiSettings.APIVersion1)]
    [Route(Settings.APIDefaultRoute + "/student")]
    [ApiController]
   [Common.Attributes.Permission(roles: new string[] { nameof(EnumRole.Student), nameof(EnumRole.StudentCampus) })]
    public class StudentController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<StudentController> _logger;

        public StudentController(IMediator mediator, ILogger<StudentController> logger)
        {
            _mediator = mediator;
            _logger = logger;
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
        /// Student Setting
        /// </summary>
        [HttpGet("student-setting")]
        [ProducesResponseType(typeof(MethodResult<StudentSettingModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> StudentSetting()
        {
            //_logger.LogError("StudentSetting: " + Request.HttpContext.ToCurl());
            MethodResult<StudentSettingModel> queryResult = await _mediator.Send(new GetStudentSettingQuery()).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Student Setting
        /// </summary>
        [HttpGet("student-setting/{userId}")]
        [ProducesResponseType(typeof(MethodResult<StudentSettingModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> StudentSettingByUserId([FromRoute] Guid? userId)
        {
            MethodResult<StudentSettingModel> queryResult = await _mediator.Send(new GetStudentSettingQuery { UserId = userId }).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }
    }
}
