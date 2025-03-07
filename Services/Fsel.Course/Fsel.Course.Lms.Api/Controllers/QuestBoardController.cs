// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Api.Controllers
{
    using System.Net;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Constants;
    using Fsel.Course.Lms.Application.Queries.QuestBoardQuery;
    using Fsel.Shared.Models.ShareModels;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Attributes;
    using Fsel.Course.Domain.Models.EntityModels;

    [ApiVersions(ApiSettings.APIVersion1)]
    [Route(Settings.APIDefaultRoute + "/quest-board")]
    [ApiController]
    public class QuestBoardController : ControllerBase
    {
        private readonly IMediator _mediator;

        public QuestBoardController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// get percent video result now
        /// </summary>
        [HttpGet("percent-video")]
        [ProducesResponseType(typeof(MethodResult<QuestBoardCategoryModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetPercentVideoResultNow([FromQuery] GetFinishOneLessonQuery query)
        {
            MethodResult<QuestBoardCategoryModel> queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// get percent unit result now
        /// </summary>
        [HttpGet("percent-unit")]
        [ProducesResponseType(typeof(MethodResult<QuestBoardCategoryModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetPercentUnitResult([FromQuery] GetFinishOneUnitQuery query)
        {
            MethodResult<QuestBoardCategoryModel> queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// get percent course result
        /// </summary>
        [HttpGet("percent-course")]
        [ProducesResponseType(typeof(MethodResult<QuestBoardCategoryModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetPercentCourseResult([FromQuery] GetFinishOneLevelPassQuery query)
        {
            MethodResult<QuestBoardCategoryModel> queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// get percent course result
        /// </summary>
        [HttpGet("questboard-param")]
        [ProducesResponseType(typeof(MethodResult<QuestBoardParamModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetQuestBoardParam([FromQuery] GetQuestBoardParamQuery query)
        {
            MethodResult<QuestBoardParamModel> queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }
    }
}
