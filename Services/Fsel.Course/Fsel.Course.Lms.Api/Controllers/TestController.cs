// Copyright (c) Atlantic. All rights reserved.

using System.Net;
using Fsel.Common.ActionResults;
using Fsel.Common.Constants;
using Fsel.Core.Base.Interfaces;
using Fsel.Course.Lms.Application.Commands.VideoTimeCodeAnswerCmd;
using Fsel.Shared.Constants;
using Fsel.Shared.Models.ShareModels;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Fsel.Course.Lms.Api.Controllers
{
    [ApiVersion(Settings.APIVersion)]
    [Route(Settings.APIDefaultRoute + "/test")]
    [ApiController]
    public class TestController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IQueueProvider _queueProvider;

        public TestController(IMediator mediator, IQueueProvider queueProvider)
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
        public IActionResult Search()
        {
            MethodResult<string> queryResult = new MethodResult<string> { Result = nameof(Search) };
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Delete Video Time Code Answers
        /// </summary>
        [HttpPut("delete-video-time-code-answers")]
        [ProducesResponseType(typeof(MethodResult<bool>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Delete([FromQuery] DeleteVideoTimeCodeAnswersCommand command)
        {
            MethodResult<bool> queryResult = await _mediator.Send(command).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Delete Video Time Code Answers
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(MethodResult<bool>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Post()
        {
            await _queueProvider.Publish(QueueSettings.RealtimeQueue.NameQueue.DiscussionBoard, new DiscussionBoardQueueModel
            {
                ObjectId = Guid.NewGuid(),
            }, CancellationToken.None);

            MethodResult<bool> queryResult = new MethodResult<bool>();
            return queryResult.GetActionResult();
        }
    }
}
