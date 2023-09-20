// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Api.Controllers
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
        /// Search ManageProgress
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(MethodResult<PagingItemsModel<StudentManageProgressModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Search([FromQuery] SearchStudentStudentProgressQuery query)
        {
            MethodResult<PagingItemsModel<StudentManageProgressModel>> queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Get Manage Student Course
        /// </summary>
        [HttpGet("manage-student-course")]
        [ProducesResponseType(typeof(MethodResult<CourseProgressModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetManageStudentCourse([FromQuery] GetStudentProgressCourseQuery query)
        {
            MethodResult<CourseProgressModel> queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Get Manage Student Lesson
        /// </summary>
        [HttpGet("manage-student-lesson")]
        [ProducesResponseType(typeof(MethodResult<IList<LessonManagerProgressModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetManageStudentLesson([FromQuery] GetStudentManageLessonQuery query)
        {
            MethodResult<IList<LessonManagerProgressModel>> queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Get Manage Student Unit
        /// </summary>
        [HttpGet("manage-student-unit")]
        [ProducesResponseType(typeof(MethodResult<IList<UnitManagerProgressModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetManageStudentUnit([FromQuery] GetStudentManageUnitQuery query)
        {
            MethodResult<IList<UnitManagerProgressModel>> queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Get Manage Student Progress Course
        /// </summary>
        [HttpGet("manage-student-progress-course")]
        [ProducesResponseType(typeof(MethodResult<CourseManagerProgressModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetManageStudentProgressCourse([FromQuery] GetStudentManageProgressCourseQuery query)
        {
            MethodResult<CourseManagerProgressModel> queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Get Manage Student Progress Unit
        /// </summary>
        [HttpGet("manage-student-progress-unit")]
        [ProducesResponseType(typeof(MethodResult<UnitManagerProgressModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetManageStudentProgressUnit([FromQuery] GetStudentManageProgressUnitQuery query)
        {
            MethodResult<UnitManagerProgressModel> queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }
    }
}
