using System.Net;
using Asp.Versioning;
using Fsel.Common.ActionResults;
using Fsel.Common.Attributes;
using Fsel.Common.Constants;
using Fsel.Sender.Application.Commands.SendZaloCmd;
using Fsel.Shared.Constants;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Fsel.Sender.Api.Controllers
{
    [ApiVersion(ApiSettings.APIVersion1)]
    [ApiVersion(ApiSettings.APIVersion1i1)]
    [Route(Settings.APIDefaultRoute + "/zalo/send-sms")]
    [ApiController]
    [Permission]
    public class ZaloController : ControllerBase
    {
        private IMediator _mediator;

        public ZaloController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Send SMS
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(MethodResult<bool>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> SendSMSByZalo([FromBody] SendSMSByZaloCommand command)
        {
            var commandResult = await _mediator.Send(command).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }
    }
}
