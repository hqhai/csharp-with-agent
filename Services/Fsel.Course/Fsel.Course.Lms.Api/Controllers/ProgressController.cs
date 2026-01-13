// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Api.Controllers
{
    using System.Collections.Generic;
    using System.Net;
    using Asp.Versioning;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Constants;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Lms.Application.Queries.HomeWorkQuery;
    using Fsel.Course.Lms.Application.Queries.LessonQuery;
    using Fsel.Course.Lms.Application.Queries.ProgressQuery;
    using Fsel.Course.Lms.Application.Queries.StudentProgressQuery;
    using Fsel.Shared.Attributes;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;

    [ApiVersions(ApiSettings.APIVersion1)]
    [Route(Settings.APIDefaultRoute + "/progress")]
    [ApiController]
    [Common.Attributes.Permission]
    public class ProgressController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ProgressController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// get Course progress
        /// </summary>
        [HttpGet("course")]
        [MapToApiVersion(ApiSettings.APIVersion1)]
        [MapToApiVersion(ApiSettings.APIVersion1i1)]
        [ProducesResponseType(typeof(MethodResult<IList<CourseUnitMockTestResultModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetCourseProgress([FromQuery] GetCourseUnitMockTestByCourseQuery query)
        {
            MethodResult<IList<CourseUnitMockTestResultModel>> queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// get overall
        /// </summary>
        [HttpGet("overall/{courseId}")]
        [MapToApiVersion(ApiSettings.APIVersion1)]
        [MapToApiVersion(ApiSettings.APIVersion1i1)]
        [ProducesResponseType(typeof(MethodResult<OverallScoreModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetOverallScore([FromRoute] Guid courseId)
        {
            MethodResult<OverallScoreModel> queryResult = await _mediator.Send(new GetOverallScoreQuery { CourseId = courseId }).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// get progress menu
        /// </summary>
        [HttpGet("progress-menu/{courseId}")]
        [MapToApiVersion(ApiSettings.APIVersion1)]
        [MapToApiVersion(ApiSettings.APIVersion1i1)]
        [ProducesResponseType(typeof(MethodResult<ProgressMenuModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetProgressMenu([FromRoute] Guid courseId)
        {
            MethodResult<ProgressMenuModel> queryResult = await _mediator.Send(new GetProgressMenuQuery { CourseId = courseId }).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// get overall mock test
        /// </summary>
        [HttpGet("overall-mock-test/{courseId}")]
        [MapToApiVersion(ApiSettings.APIVersion1)]
        [MapToApiVersion(ApiSettings.APIVersion1i1)]
        [ProducesResponseType(typeof(MethodResult<IList<OverallScoreReportByMockTestModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetOverallScoreByMockTest([FromRoute] Guid courseId)
        {
            MethodResult<IList<OverallScoreReportByMockTestModel>> queryResult = await _mediator.Send(new GetOverallScoreByMockTestQuery { CourseId = courseId }).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// get overall detail mock test
        /// </summary>
        [HttpGet("overall-detail-mock-test")]
        [MapToApiVersion(ApiSettings.APIVersion1)]
        [MapToApiVersion(ApiSettings.APIVersion1i1)]
        [ProducesResponseType(typeof(MethodResult<MockTestResultModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetDetailOverallScoreByMockTest([FromQuery] GetDetailOverallScoreByMockTestQuery query)
        {
            MethodResult<MockTestResultModel> queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// get overall homework
        /// </summary>
        [HttpGet("overall-homework/{courseId}")]
        [MapToApiVersion(ApiSettings.APIVersion1)]
        [MapToApiVersion(ApiSettings.APIVersion1i1)]
        [ProducesResponseType(typeof(MethodResult<OverallScoreReportModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetOverallScoreByHomeWork([FromRoute] Guid courseId)
        {
            MethodResult<OverallScoreReportModel> queryResult = await _mediator.Send(new GetOverallScoreByHomeWorkQuery { CourseId = courseId }).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// get overall finalTest
        /// </summary>
        [HttpGet("overall-final-test/{courseId}")]
        [MapToApiVersion(ApiSettings.APIVersion1)]
        [MapToApiVersion(ApiSettings.APIVersion1i1)]
        [ProducesResponseType(typeof(MethodResult<OverallScoreReportModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetOverallScoreByFinalTest([FromRoute] Guid courseId)
        {
            MethodResult<OverallScoreReportModel> queryResult = await _mediator.Send(new GetOverallScoreByFinalTestQuery { CourseId = courseId }).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// get overall lesson
        /// </summary>
        [HttpGet("overall-lesson/{courseId}")]
        [MapToApiVersion(ApiSettings.APIVersion1)]
        [MapToApiVersion(ApiSettings.APIVersion1i1)]
        [ProducesResponseType(typeof(MethodResult<OverallScoreReportModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetOverallScoreByLesson([FromRoute] Guid courseId)
        {
            MethodResult<OverallScoreReportModel> queryResult = await _mediator.Send(new GetOverallScoreByLessonQuery { CourseId = courseId }).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// get overall lesson
        /// </summary>
        [HttpGet("overall-unit-test/{courseId}")]
        [MapToApiVersion(ApiSettings.APIVersion1)]
        [MapToApiVersion(ApiSettings.APIVersion1i1)]
        [ProducesResponseType(typeof(MethodResult<OverallScoreReportModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetOverallScoreByUnitTest([FromRoute] Guid courseId)
        {
            MethodResult<OverallScoreReportModel> queryResult = await _mediator.Send(new GetOverallScoreByUnitTestQuery { CourseId = courseId }).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// get overall lesson
        /// </summary>
        [HttpGet("overall-class-forum/{courseId}")]
        [MapToApiVersion(ApiSettings.APIVersion1)]
        [MapToApiVersion(ApiSettings.APIVersion1i1)]
        [ProducesResponseType(typeof(MethodResult<OverallScoreReportModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetOverallScoreByClassForum([FromRoute] Guid courseId)
        {
            MethodResult<OverallScoreReportModel> queryResult = await _mediator.Send(new GetOverallScoreByClassForumQuery { CourseId = courseId }).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// get unit by unit
        /// </summary>
        [HttpGet("unit")]
        [MapToApiVersion(ApiSettings.APIVersion1)]
        [MapToApiVersion(ApiSettings.APIVersion1i1)]
        [ProducesResponseType(typeof(MethodResult<IList<UnitModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetUnitByUnit([FromQuery] GetUnitByUnitQuery query)
        {
            MethodResult<IList<UnitModel>> queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Get unit by unit Test
        /// </summary>
        [HttpGet("unit/unit-test")]
        [MapToApiVersion(ApiSettings.APIVersion1)]
        [MapToApiVersion(ApiSettings.APIVersion1i1)]
        [ProducesResponseType(typeof(MethodResult<OverallScoreReportModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetUnitByUnitTest([FromQuery] GetUnitByUnitTestQuery query)
        {
            MethodResult<OverallScoreReportModel> queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// get unit class forum
        /// </summary>
        [HttpGet("unit/class-forum")]
        [MapToApiVersion(ApiSettings.APIVersion1)]
        [MapToApiVersion(ApiSettings.APIVersion1i1)]
        [ProducesResponseType(typeof(MethodResult<IList<ClassForumReportModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetUnitByClassForum([FromQuery] GetUnitByClassForumQuery query)
        {
            MethodResult<IList<ClassForumReportModel>> queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// get unit class forum detail
        /// </summary>
        [HttpGet("unit/class-forum-detail/{classForumId}")]
        [MapToApiVersion(ApiSettings.APIVersion1)]
        [MapToApiVersion(ApiSettings.APIVersion1i1)]
        [ProducesResponseType(typeof(MethodResult<IList<ClassForumScoreModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetUnitByClassForumDetail([FromRoute] Guid classForumId)
        {
            MethodResult<IList<ClassForumScoreModel>> queryResult = await _mediator.Send(new GetUnitByClassForumDetailQuery { ClassForumId = classForumId }).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// get unit class forum detail
        /// </summary>
        [HttpGet("unit/class-forum-detail")]
        [MapToApiVersion(ApiSettings.APIVersion1)]
        [MapToApiVersion(ApiSettings.APIVersion1i1)]
        [ProducesResponseType(typeof(MethodResult<IList<ClassForumAIModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetUnitByClassForumDetail([FromQuery] GetUnitByClassForumDtoQuery query)
        {
            MethodResult<IList<ClassForumAIModel>> queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Get List lesson by unitId
        /// </summary>
        [HttpGet("unit/lessons")]
        [MapToApiVersion(ApiSettings.APIVersion1)]
        [MapToApiVersion(ApiSettings.APIVersion1i1)]
        [ProducesResponseType(typeof(MethodResult<IList<LessonModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetLessonsByUnitId([FromQuery] GetListLessonQuery query)
        {
            MethodResult<IList<LessonModel>> queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Get List lesson by unitId
        /// </summary>
        [HttpGet("unit/lessons-total")]
        [MapToApiVersion(ApiSettings.APIVersion1)]
        [MapToApiVersion(ApiSettings.APIVersion1i1)]
        [ProducesResponseType(typeof(MethodResult<UnitResultModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetUnitByLessonTotal([FromQuery] GetUnitByLessonTotalQuery query)
        {
            MethodResult<UnitResultModel> queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Get List lesson by lessonResultId
        /// </summary>
        [HttpGet("unit/lessons/{lessonResultId}")]
        [MapToApiVersion(ApiSettings.APIVersion1)]
        [MapToApiVersion(ApiSettings.APIVersion1i1)]
        [ProducesResponseType(typeof(MethodResult<OverallScoreReportModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetUnitByLesson([FromRoute] Guid lessonResultId)
        {
            MethodResult<OverallScoreReportModel> queryResult = await _mediator.Send(new GetUnitByLessonQuery { LessonResultId = lessonResultId }).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Get lesson homework score
        /// </summary>
        [HttpGet("unit/homework/{lessonResultId}")]
        [MapToApiVersion(ApiSettings.APIVersion1)]
        [MapToApiVersion(ApiSettings.APIVersion1i1)]
        [ProducesResponseType(typeof(MethodResult<IList<LessonHomeWorkResultModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetListHomeWork([FromRoute] Guid lessonResultId)
        {
            MethodResult<IList<LessonHomeWorkResultModel>> queryResult = await _mediator.Send(new GetListHomeworkQuery { LessonResultId = lessonResultId }).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// get studentprogress
        /// </summary>
        [HttpPost("students-competition")]
        [ProducesResponseType(typeof(MethodResult<IList<CompetitionStudentProgressModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetListStudentProgress([FromBody] GetStudentsProgressCoursesQuery query)
        {
            MethodResult<IList<CompetitionStudentProgressModel>> queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Report Student
        /// </summary>
        [HttpPost("export-report-student")]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [Common.Attributes.Permission(role: nameof(EnumRole.Admin))]
        public async Task<IActionResult> Export([FromQuery] ExportEmailStudentByProgressQuery query)
        {
            MethodResult<Stream> commandResult = await _mediator.Send(query).ConfigureAwait(false);
            if (!commandResult.IsOK || commandResult.Result == null)
            {
                return commandResult.GetActionResult();
            }
            return File(commandResult.Result, Settings.Excels.ContentType, "report_student_export.xlsx");
        }

        /// <summary>
        /// Report Student
        /// </summary>
        [HttpPost("export-report-progress-student")]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [Common.Attributes.Permission(role: nameof(EnumRole.Admin))]
        public async Task<IActionResult> ExportProgressStudent([FromQuery] ExportFullNameByReportProgressQuery query)
        {
            MethodResult<Stream> commandResult = await _mediator.Send(query).ConfigureAwait(false);
            if (!commandResult.IsOK || commandResult.Result == null)
            {
                return commandResult.GetActionResult();
            }
            return File(commandResult.Result, Settings.Excels.ContentType, "report_progress_student_export.xlsx");
        }
    }
}
