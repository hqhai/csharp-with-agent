// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Api.Controllers.V1i2
{
    using System.Collections.Generic;
    using System.Net;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Constants;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Lms.Application.Queries.HomeWorkQuery;
    using Fsel.Course.Lms.Application.Queries.LessonQuery.V1i2;
    using Fsel.Course.Lms.Application.Queries.ProgressQuery.V1i2;
    using Fsel.Course.Lms.Application.Queries.ProgressQuery.V1i2.Overall;
    using Fsel.Course.Lms.Application.Queries.ProgressQuery.V1i2.Unit;
    using Fsel.Shared.Attributes;
    using Fsel.Shared.Constants;
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
        /// get overall
        /// </summary>
        [HttpGet("overall/{courseId}")]
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
        [ProducesResponseType(typeof(MethodResult<ProgressMenuModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetProgressMenu([FromRoute] Guid courseId)
        {
            MethodResult<ProgressMenuModel> queryResult = await _mediator.Send(new GetProgressMenuQuery { CourseId = courseId }).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// get overall homework
        /// </summary>
        [HttpGet("overall-homework/{courseId}")]
        [ProducesResponseType(typeof(MethodResult<OverallScoreReportModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetOverallScoreByHomeWork([FromRoute] Guid courseId)
        {
            MethodResult<OverallScoreReportModel> queryResult = await _mediator.Send(new GetOverallScoreByHomeWorkQuery { CourseId = courseId }).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// get overall lesson
        /// </summary>
        [HttpGet("overall-lesson/{courseId}")]
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
        [HttpGet("unit/class-forum-detail/{classForumResultId}")]
        [ProducesResponseType(typeof(MethodResult<IList<ClassForumScoreModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetUnitByClassForumDetail([FromRoute] Guid classForumResultId)
        {
            var queryResult = await _mediator.Send(new GetUnitByClassForumDetailQuery { ClassForumResultId = classForumResultId }).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// get unit class forum detail
        /// </summary>
        [HttpGet("unit/class-forum-detail")]
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
        [ProducesResponseType(typeof(MethodResult<IList<LessonHomeWorkResultModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetListHomeWork([FromRoute] Guid lessonResultId)
        {
            MethodResult<IList<LessonHomeWorkResultModel>> queryResult = await _mediator.Send(new GetListHomeworkQuery { LessonResultId = lessonResultId }).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Get lesson homework score
        /// </summary>
        [HttpGet("learning-pathway/{courseId}")]
        [ProducesResponseType(typeof(MethodResult<IList<LessonHomeWorkResultModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Get([FromRoute] Guid courseId)
        {
            MethodResult<LearningPathwayResponse> queryResult = await _mediator.Send(new GetLearningPathwayQuery
            {
                CourseId = courseId
            }).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }
    }
}
