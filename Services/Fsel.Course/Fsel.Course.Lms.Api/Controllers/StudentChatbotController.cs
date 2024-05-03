// Copyright (c) Atlantic. All rights reserved.

using System.Net;
using Asp.Versioning;
using Fsel.Common.ActionResults;
using Fsel.Common.Constants;
using Fsel.Core.Base.Interfaces;
using Fsel.Course.Lms.Application.Commands.StudentChatbotCmd;
using Fsel.Course.Lms.Application.Commands.TestCmd;
using Fsel.Course.Lms.Application.Commands.VideoTimeCodeAnswerCmd;
using Fsel.Shared.Constants;
using Fsel.Shared.Models.ShareModels;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Fsel.Course.Lms.Api.Controllers
{
    [ApiVersion(ApiSettings.APIVersion1)]
    [ApiVersion(ApiSettings.APIVersion1i1)]
    [Route(Settings.APIDefaultRoute + "/student-chat-bot")]
    [ApiController]
    public class StudentChatbotController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IQueueProvider _queueProvider;

        public StudentChatbotController(IMediator mediator, IQueueProvider queueProvider)
        {
            _mediator = mediator;
            _queueProvider = queueProvider;
        }

        /// <summary>
        /// Search Course
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(MethodResult<string>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [MapToApiVersion(ApiSettings.APIVersion1)]
        public IActionResult Search()
        {
            var a = TimeZoneInfo.GetSystemTimeZones();
            MethodResult<string> queryResult = new MethodResult<string> { Result = nameof(Search) };
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Create video time code answer
        /// </summary>
        [HttpPost("chat-bot-practice")]
        [ProducesResponseType(typeof(MethodResult<bool>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [MapToApiVersion(ApiSettings.APIVersion1)]
        public async Task<IActionResult> CreateVideoTimeCodeAnswer([FromBody] CreateChatbotAudioCommand query)
        {
            MethodResult<bool> commandResult = await _mediator.Send(query).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }
    }
}
