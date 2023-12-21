// Copyright (c) Atlantic. All rights reserved.

using System.Net;
using Fsel.Common.ActionResults;
using Fsel.Common.Constants;
using Fsel.Sender.Application.Commands.SendEmailCmd;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Asp.Versioning;
using Fsel.Shared.Constants;

namespace Fsel.Sender.Api.Controllers
{
    [ApiVersion(ApiSettings.APIVersion1)][ApiVersion(ApiSettings.APIVersion1i1)]
    [Route(Settings.APIDefaultRoute + "/send-email")]
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

        /// <summary>
        /// SendMail
        /// </summary>
        [HttpPost("send-by-template")]
        [ProducesResponseType(typeof(MethodResult<bool>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> SendEmail([FromBody] SendEmailByTemplateCommand command)
        {
            MethodResult<bool> commandResult = await _mediator.Send(command).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }
    }
}
