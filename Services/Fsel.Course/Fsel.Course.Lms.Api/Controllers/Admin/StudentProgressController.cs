// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Api.Controllers.Admin
{
    using System.Net;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Attributes;
    using Fsel.Common.Constants;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Lms.Application.Queries.CourseQuery.V1i1;
    using Fsel.Course.Lms.Application.Queries.StudentProgressQuery;
    using Fsel.Course.Lms.Application.Queries.StudentQuery;
    using Fsel.Shared.Attributes;
    using Fsel.Shared.Constants;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;

    [ApiVersions(ApiSettings.APIVersion1)]
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
        /// Get Manage Student Courses
        /// </summary>
        [HttpGet("courses")]
        [ProducesResponseType(typeof(MethodResult<IList<CourseStudentProgressModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetManageStudentCourse([FromQuery] GetStudentProgressCoursesQuery query)
        {
            MethodResult<IList<CourseStudentProgressModel>> queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Get Manage Student Lessons
        /// </summary>
        [HttpGet("lessons")]
        [ProducesResponseType(typeof(MethodResult<IList<LessonStudentProgressModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetManageStudentLesson([FromQuery] GetStudentProgressLessonsQuery query)
        {
            MethodResult<IList<LessonStudentProgressModel>> queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Get Student Units
        /// </summary>
        [HttpGet("units")]
        [ProducesResponseType(typeof(MethodResult<IList<UnitStudentProgressModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetManageStudentUnit([FromQuery] GetStudentProgressUnitsQuery query)
        {
            MethodResult<IList<UnitStudentProgressModel>> queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Get Manage Student Progress Course
        /// </summary>
        [HttpGet("course")]
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
        [HttpGet("unit")]
        [ProducesResponseType(typeof(MethodResult<UnitStudentProgressModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetStudentProgressUnit([FromQuery] GetStudentProgressUnitQuery query)
        {
            MethodResult<UnitStudentProgressModel> queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Get Student Progress Final
        /// </summary>
        [HttpGet("final-test")]
        [ProducesResponseType(typeof(MethodResult<UnitStudentProgressModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetStudentProgressFinalTest([FromQuery] GetStudentProgressFinalTestQuery query)
        {
            MethodResult<UnitStudentProgressModel> queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Get Student Progress Final
        /// </summary>
        [HttpGet("mock-tests")]
        [ProducesResponseType(typeof(MethodResult<IList<UnitStudentProgressModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetStudentProgressMockTests([FromQuery] GetStudentProgressMockTestsQuery query)
        {
            MethodResult<IList<UnitStudentProgressModel>> queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Get Student Progress HOmeWork
        /// </summary>
        [HttpGet("home-work")]
        [ProducesResponseType(typeof(MethodResult<HomeWorkStudentProgressModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetStudentProgressHomeWork([FromQuery] GetStudentProgressHomeWorkQuery query)
        {
            MethodResult<HomeWorkStudentProgressModel> queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Get Student Progress HomeWork
        /// </summary>
        [HttpGet("class-forum")]
        [ProducesResponseType(typeof(MethodResult<IList<ClassForumStudentProgressModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetStudentProgressClassForum([FromQuery] GetStudentProgressClassForumQuery query)
        {
            var queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Get Student Progress Video
        /// </summary>
        [HttpGet("video")]
        [ProducesResponseType(typeof(MethodResult<IList<VideoStudentProgressModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetStudentProgressVideo([FromQuery] GetStudentProgressVideoQuery query)
        {
            var queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Get Manage Courses
        /// </summary>
        [HttpGet("level-selection")]
        [ProducesResponseType(typeof(MethodResult<IList<LevelDtoModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetLevelSelection([FromQuery] GetLevelSelectionQuery query)
        {
            var commandResult = await _mediator.Send(query).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Get Learning Report
        /// </summary>
        [HttpGet("learning-report")]
        [ProducesResponseType(typeof(MethodResult<StudentCourseProgressModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Get([FromQuery] GetStudentLearningReportQuery query)
        {
            var commandResult = await _mediator.Send(query).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }
    }
}
