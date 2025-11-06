// Copyright (c) Atlantic. All rights reserved.

using System.Net;
using Asp.Versioning;
using Fsel.Common.ActionResults;
using Fsel.Common.Constants;
using Fsel.Sender.Application.Commands.SendEmailCmd;
using Fsel.Sender.Application.Queries;
using Fsel.Shared.Constants;
using Fsel.Shared.Models.SenderTemplates;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Fsel.Sender.Api.Controllers
{
    [ApiVersion(ApiSettings.APIVersion1)]
    [ApiVersion(ApiSettings.APIVersion1i1)]
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

        /// <summary>
        /// send mail using smtp
        /// </summary>
        [HttpPost("send-mail-using-smtp")]
        [ProducesResponseType(typeof(MethodResult<bool>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> SendEmailUsingSMTP([FromBody] SendMailUsingSMTPCommand command)
        {
            var commandResult = await _mediator.Send(command).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// send mail by template using smtp
        /// </summary>
        [HttpPost("send-mail-by-template-using-smtp")]
        [ProducesResponseType(typeof(MethodResult<bool>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> SendEmailByTemplateUsingSMTP([FromBody] SendMailByTemplateUsingSMTPCommand command)
        {
            var commandResult = await _mediator.Send(command).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// SendMail
        /// </summary>
        [HttpPost("send-with-attachments")]
        [ProducesResponseType(typeof(MethodResult<bool>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> SendWithAttachments([FromQuery] IList<string> toEmails, [FromQuery] IList<string>? bccEmails, [FromQuery] IList<string>? ccEmails, [FromQuery] string? subject, [FromQuery] string? content, IList<IFormFile>? attachments)
        {
            MethodResult<bool> commandResult = await _mediator.Send(new SendEmailWithAttachmentsCommand()
            {
                ToEmails = toEmails,
                BccEmails = bccEmails ?? new List<string>(),
                CcEmails = ccEmails ?? new List<string>(),
                Content = content,
                Subject = subject,
                Attachments = attachments
            }).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// get history send mail learning progress
        /// </summary>
        [HttpPost("get-histories-send-mail-learning-progress")]
        [ProducesResponseType(typeof(MethodResult<IList<HistorySendMailLearningProgressModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetHistorySendMailLearningProgress([FromBody] GetHistoriesSendMailLearningProgressQuery query)
        {
            var commandResult = await _mediator.Send(query).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }
    }
}
