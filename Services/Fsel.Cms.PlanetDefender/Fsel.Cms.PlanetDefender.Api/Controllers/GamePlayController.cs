// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Cms.PlanetDefender.Api.Controllers
{
    using System.Net;
    using Fsel.Cms.PlanetDefender.Application.Commands.GameHistoryCmd;
    using Fsel.Cms.PlanetDefender.Application.Queries.RandomQuestionsQuery;
    using Fsel.Cms.PlanetDefender.Application.Services.SystemServices.Models;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Constants;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;

    [ApiVersion(Settings.APIVersion)]
    [Route(Settings.APIDefaultRoute + "/game-play")]
    [ApiController]
    public class GamePlayController : ControllerBase
    {
        private readonly IMediator _mediator;

        public GamePlayController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// random questions
        /// </summary>
        [HttpGet("random-questions")]
        [ProducesResponseType(typeof(MethodResult<IList<GameVocabularyModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> RandomQuestions([FromQuery] RandomQuestionsQuery query)
        {
            var commandResult = await _mediator.Send(query).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// answer the question
        /// </summary>
        [HttpPost("answer-the-question")]
        [ProducesResponseType(typeof(MethodResult<bool>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> AnswerTheQuestion([FromBody] AnswerTheQuestionCommand command)
        {
            var commandResult = await _mediator.Send(command).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }
    }
}
