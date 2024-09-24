// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Api.Controllers
{
    using Asp.Versioning;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Constants;
    using Fsel.Shared.Attributes;
    using Fsel.Shared.Constants;
    using Fsel.System.Application.Commands.SendMail;
    using global::System.Net;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;

    [ApiVersion(ApiSettings.APIVersion1)]
    [ApiVersion(ApiSettings.APIVersion1i1)]
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
        /// Send Mails Marketing
        /// </summary>
        [HttpPost("send-mails-marketing")]
        [DisableRequestSizeLimit]
        [RequestFormLimits(ValueLengthLimit = int.MaxValue, MultipartBodyLengthLimit = long.MaxValue)]
        [ProducesResponseType(typeof(MethodResult<bool>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> SendMailsMarketing([FromQuery] string? emailTest, [FromQuery] string subject, IFormFile fileTemplate, IList<IFormFile>? attachments)
        {
            var commandResult = await _mediator.Send(new SendMailsMarketingCommand
            {
                EmailTest = emailTest,
                Subject = subject,
                Template = fileTemplate,
                Attachments = attachments
            }).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }
    }
}
