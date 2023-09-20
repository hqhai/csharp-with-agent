// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Api.Controllers.Admin
{
    using System.Net;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Constants;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Lms.Application.Queries.StudentProgressQuery;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;

    [ApiVersion(Settings.APIVersion)]
    [Route(Settings.APIDefaultRoute + "/student-progress")]
    [ApiController]
    public class StudentProgressController : ControllerBase
    {
        private readonly IMediator _mediator;

        public StudentProgressController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Search Student Progress
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(MethodResult<PagingItemsModel<StudentProgressModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Search([FromQuery] SearchStudentProgressQuery query)
        {
            MethodResult<PagingItemsModel<StudentProgressModel>> queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Get Manage Student Course
        /// </summary>
        [HttpGet("student-course")]
        [ProducesResponseType(typeof(MethodResult<IList<CourseStudentProgressModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetManageStudentCourse([FromQuery] GetStudentCoursesQuery query)
        {
            MethodResult<IList<CourseStudentProgressModel>> queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Get Manage Student Lesson
        /// </summary>
        [HttpGet("student-lesson")]
        [ProducesResponseType(typeof(MethodResult<IList<LessonStudentProgressModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetManageStudentLesson([FromQuery] GetStudentLessonsQuery query)
        {
            MethodResult<IList<LessonStudentProgressModel>> queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Get Student Unit
        /// </summary>
        [HttpGet("student-unit")]
        [ProducesResponseType(typeof(MethodResult<IList<UnitStudentProgressModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetManageStudentUnit([FromQuery] GetStudentUnitsQuery query)
        {
            MethodResult<IList<UnitStudentProgressModel>> queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Get Manage Student Progress Course
        /// </summary>
        [HttpGet("student-progress-course")]
        [ProducesResponseType(typeof(MethodResult<CourseStudentProgressModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetManageStudentProgressCourse([FromQuery] GetStudentProgressCourseQuery query)
        {
            MethodResult<CourseStudentProgressModel> queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Get Student Progress Unit
        /// </summary>
        [HttpGet("student-progress-unit")]
        [ProducesResponseType(typeof(MethodResult<UnitStudentProgressModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetManageStudentProgressUnit([FromQuery] GetStudentProgressUnitQuery query)
        {
            MethodResult<UnitStudentProgressModel> queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }
    }
}
