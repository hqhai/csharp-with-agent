// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Api.Controllers
{
    using System.Net;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Constants;
    using Fsel.Course.Lms.Application.Queries.QuestBoardQuery;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;

    [ApiVersion(Settings.APIVersion)]
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
        [ProducesResponseType(typeof(MethodResult<double>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetPercentVideoResultNow([FromQuery] GetFinishOneLessonQuery query)
        {
            MethodResult<double> queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// get percent unit result now
        /// </summary>
        [HttpGet("percent-unit")]
        [ProducesResponseType(typeof(MethodResult<double>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetPercentUnitResult([FromQuery] GetFinishOneUnitQuery query)
        {
            MethodResult<double> queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// get percent course result
        /// </summary>
        [HttpGet("percent-course")]
        [ProducesResponseType(typeof(MethodResult<double>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetPercentCourseResult([FromQuery] GetFinishOneLevelPassQuery query)
        {
            MethodResult<double> queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }
    }
}
