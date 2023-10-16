// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Api.Controllers
{
    using System.Net;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Constants;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Lms.Application.Commands.LessonCmd;
    using Fsel.Course.Lms.Application.Queries.LessonQuery;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;

    [ApiVersion(Settings.APIVersion)]
    [Route(Settings.APIDefaultRoute + "/lesson")]
    [Common.Attributes.Permission(role: nameof(EnumRole.Student))]
    [ApiController]
    public class LessonController : ControllerBase
    {
        private readonly IMediator _mediator;

        public LessonController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Get List lesson by unit
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(MethodResult<LessonsMockTestModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetLesson([FromQuery] GetLessonQuery query)
        {
            MethodResult<LessonsMockTestModel> queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Get Lesson score
        /// </summary>
        [HttpGet("get-lesson-score")]
        [ProducesResponseType(typeof(MethodResult<LessonScoreModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetLessonScore([FromQuery] GetLessonScoreQuery query)
        {
            MethodResult<LessonScoreModel> queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Start Lesson
        /// </summary>
        [HttpPost("start-lesson")]
        [ProducesResponseType(typeof(MethodResult<LessonResultModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> StartLesson([FromBody] StartLessonCommand query)
        {
            MethodResult<LessonResultModel> queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }
    }
}
