using System.Net;
using Asp.Versioning;
using Fsel.Common.ActionResults;
using Fsel.Common.Attributes;
using Fsel.Common.Constants;
using Fsel.Shared.Constants;
using Fsel.Shared.Enums;
using Fsel.System.Application.Commands.DailyQuiz;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Fsel.System.Api.Controllers.Admins
{
    [ApiVersion(ApiSettings.APIVersion1)]
    [ApiVersion(ApiSettings.APIVersion1i1)]
    [Route(Settings.APIDefaultRoute + "/admin/daily-quiz")]
    [ApiController]
    [Permission(role: nameof(EnumRole.Admin))]
    public class DailyQuizController : ControllerBase
    {
        private readonly IMediator _mediator;

        public DailyQuizController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// create questions
        /// </summary>
        [HttpPost("create-questions")]
        [ProducesResponseType(typeof(MethodResult<bool>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Create([FromBody] CreateDailyQuizQuestionsCommand cmd)
        {
            var commandResult = await _mediator.Send(cmd).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        [RequestSizeLimit(1 * 1024 * 1024)] // 1 MB
        [RequestFormLimits(MultipartBodyLengthLimit = 1 * 1024 * 1024)]
        [HttpPost("import-questions")]
        [ProducesResponseType(typeof(MethodResult<bool>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> ImportQuestions([FromForm] ToolImportQuestionsCommand command)
        {
            MethodResult<Stream> commandResult = await _mediator.Send(command).ConfigureAwait(false);
            if (!commandResult.IsOK || commandResult.Result == null)
            {
                return commandResult.GetActionResult();
            }
            return File(commandResult.Result, Settings.Excels.ContentType, "Import_Questions_Error.xlsx");
        }

        /// <summary>
        /// choose winners
        /// </summary>
        [HttpPost("choose-winners")]
        [ProducesResponseType(typeof(MethodResult<bool>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> ChooseWinners()
        {
            var commandResult = await _mediator.Send(new ChooseDailyQuizWinnersCommand()).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }
    }
}
