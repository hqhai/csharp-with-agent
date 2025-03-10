// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Storage.Api.Controllers
{
    using System.Net;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Constants;
    using Fsel.Shared.Attributes;
    using Fsel.Shared.Constants;
    using Fsel.Storage.Application.Command.SendMails;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;

    [ApiVersions(ApiSettings.APIVersion1)]
    [Route(Settings.APIDefaultRoute + "/send-mail")]
    [ApiController]
    public class SendMailController : ControllerBase
    {
        private readonly IMediator _mediator;

        public SendMailController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Send Mails pre interview
        /// </summary>
        [DisableRequestSizeLimit]
        [RequestFormLimits(ValueLengthLimit = int.MaxValue, MultipartBodyLengthLimit = long.MaxValue)]
        [HttpPost("send-mails-pre-interview")]
        [ProducesResponseType(typeof(MethodResult<bool>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> SendMailPreInterview([FromForm] SendMailPreInterviewCommand command)
        {
            var commandResult = await _mediator.Send(command).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }
    }
}
