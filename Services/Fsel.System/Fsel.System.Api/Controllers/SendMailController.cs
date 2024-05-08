// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Api.Controllers
{
    using Asp.Versioning;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Constants;
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

        /// <summary>
        /// Send Mails Marketing
        /// </summary>
        [HttpPost("send-mails-pre-interview")]
        [ProducesResponseType(typeof(MethodResult<bool>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> SendMailPreInterview([FromQuery] string name, [FromQuery] string email, IFormFile video, IFormFile lesson, IFormFile activity)
        {
            var commandResult = await _mediator.Send(new SendMailPreInterviewCommand
            {
                Name = name,
                Email = email,
                Video = video,
                Lesson = lesson,
                Activity = activity
            }).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }
    }
}
