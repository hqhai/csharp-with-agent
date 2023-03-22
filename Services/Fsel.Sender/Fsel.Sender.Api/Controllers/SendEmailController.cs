using System.Net;
using Fsel.Common.ActionResults;
using Fsel.Common.Constants;
using Fsel.Common.Helpers;
using Fsel.Sender.Application.Commands.SendEmailCmd;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Fsel.Sender.Api.Controllers
{
    [ApiVersion(Settings.APIVersion)]
    [Route(Settings.APIDefaultRoute + "/sender")]
    [ApiController]
    public class SendEmailController : ControllerBase
    {
        private IMediator _mediator;

        public SendEmailController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// SendMail
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(MethodResult<bool>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> SendEmail([FromBody] SendEmailCommand command)
        {
            MethodResult<bool> commandResult = await _mediator.Send(command).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }
    }
}
