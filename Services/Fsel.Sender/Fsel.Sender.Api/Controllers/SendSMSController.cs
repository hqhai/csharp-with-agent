// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Sender.Api.Controllers
{
    using System.Net;
    using Asp.Versioning;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Constants;
    using Fsel.Sender.Application.Commands.SendSMSCmd;
    using Fsel.Shared.Constants;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;

    [ApiVersion(ApiSettings.APIVersion1)]
    [ApiVersion(ApiSettings.APIVersion1i1)]
    [Route(Settings.APIDefaultRoute + "/send-sms")]
    [ApiController]
    public class SendSMSController : ControllerBase
    {
        private IMediator _mediator;

        public SendSMSController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Send SMS
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(MethodResult<bool>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> SendSMSByIRIS([FromBody] SendSMSCommand command)
        {
            var commandResult = await _mediator.Send(command).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }
    }
}
