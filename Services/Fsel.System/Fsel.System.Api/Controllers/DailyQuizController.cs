using System.Net;
using Asp.Versioning;
using Fsel.Common.ActionResults;
using Fsel.Common.Constants;
using Fsel.Shared.Constants;
using Fsel.Shared.Enums;
using Fsel.System.Application.Commands.DailyQuiz;
using Fsel.System.Application.Queries.DailyQuiz;
using Fsel.System.Domain.Models.EntityModels;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Fsel.System.Api.Controllers
{
    [ApiVersion(ApiSettings.APIVersion1)]
    [ApiVersion(ApiSettings.APIVersion1i1)]
    [Route(Settings.APIDefaultRoute + "/daily-quiz")]
    [ApiController]
    [Common.Attributes.Permission(role: nameof(EnumRole.Student))]
    public class DailyQuizController : ControllerBase
    {
        private readonly IMediator _mediator;

        public DailyQuizController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// daily quiz
        /// </summary>
        [HttpPost("daily-quiz")]
        [ProducesResponseType(typeof(MethodResult<DailyQuizModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> DailyQuiz([FromBody] DailyQuizCommand cmd)
        {
            var commandResult = await _mediator.Send(cmd).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// get winner
        /// </summary>
        [HttpGet("get-winner")]
        [ProducesResponseType(typeof(MethodResult<DailyQuizModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetWinner()
        {
            var commandResult = await _mediator.Send(new GetDailyQuizWinnerInDayQuery()).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// get questions
        /// </summary>
        [HttpGet("get-daily-quiz")]
        [ProducesResponseType(typeof(MethodResult<DailyQuizModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Get()
        {
            var queryResult = await _mediator.Send(new GetDailyQuizQuestionsQuery()).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// get questions
        /// </summary>
        [HttpGet("get-remaining-time")]
        [ProducesResponseType(typeof(MethodResult<int>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetRemainingTime()
        {
            var queryResult = await _mediator.Send(new GetRemainingTimeQuery()).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }
    }
}
