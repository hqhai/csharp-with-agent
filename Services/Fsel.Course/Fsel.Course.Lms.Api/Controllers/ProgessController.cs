// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Api.Controllers
{
    using System.Collections.Generic;
    using System.Net;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Constants;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Lms.Application.Queries.HomeWorkQuery;
    using Fsel.Course.Lms.Application.Queries.LessonQuery;
    using Fsel.Course.Lms.Application.Queries.ProgessQuery;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;

    [ApiVersion(Settings.APIVersion)]
    [Route(Settings.APIDefaultRoute + "/progess")]
    [ApiController]
    [Authorize(Roles = nameof(EnumRole.Student))]
    public class ProgessController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ProgessController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// get overall
        /// </summary>
        [HttpGet("overall")]
        [ProducesResponseType(typeof(MethodResult<OverallScoreModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetOverallScore()
        {
            MethodResult<OverallScoreModel> queryResult = await _mediator.Send(new GetOverallScoreQuery()).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// get overall mocktest
        /// </summary>
        [HttpGet("overall-mocktest/{courseId}")]
        [ProducesResponseType(typeof(MethodResult<IList<OverallScoreReportByMockTestModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetOverallScoreByMockTest([FromRoute] Guid courseId)
        {
            MethodResult<IList<OverallScoreReportByMockTestModel>> queryResult = await _mediator.Send(new GetOverallScoreByMockTestQuery { CourseId = courseId }).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// get overall detail-mocktest
        /// </summary>
        [HttpGet("overall-detail-mocktest")]
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
        [HttpGet("unit/class-forum/{unitId}")]
        [ProducesResponseType(typeof(MethodResult<IList<ClassForumReportModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetUnitByClassForum([FromRoute] Guid unitId)
        {
            MethodResult<IList<ClassForumReportModel>> queryResult = await _mediator.Send(new GetUnitByClassForumQuery { UnitId = unitId }).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Get List lesson by unitId
        /// </summary>
        [HttpGet("unit/lessons/{unitId}")]
        [ProducesResponseType(typeof(MethodResult<IList<LessonModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetLessonsByUnitId([FromRoute] Guid unitId)
        {
            MethodResult<IList<LessonModel>> queryResult = await _mediator.Send(new GetListLessonQuery { UnitId = unitId }).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Get List lesson by lessonResultId
        /// </summary>
        [HttpGet("unit/lessons/{lessonResultId}")]
        [ProducesResponseType(typeof(MethodResult<LessonResultModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetUnitByLesson([FromRoute] Guid lessonResultId)
        {
            MethodResult<LessonResultModel> queryResult = await _mediator.Send(new GetUnitByLessonQuery { LessonResultId = lessonResultId }).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Get List lesson by unitId
        /// </summary>
        [HttpGet("unit/lessons-total/{unitResultId}")]
        [ProducesResponseType(typeof(MethodResult<UnitResultModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetUnitByLessonTotal([FromRoute] Guid unitResultId)
        {
            MethodResult<UnitResultModel> queryResult = await _mediator.Send(new GetUnitByLessonTotalQuery { UnitResultId = unitResultId }).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Get lesson homework score
        /// </summary>
        [HttpGet("unit/homeworks/{lessonResultId}")]
        [ProducesResponseType(typeof(MethodResult<IList<LessonHomeWorkResultModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetListHomeWork([FromRoute] Guid lessonResultId)
        {
            MethodResult<IList<LessonHomeWorkResultModel>> queryResult = await _mediator.Send(new GetListHomeworkQuery { LessonResultId = lessonResultId }).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }
    }
}
