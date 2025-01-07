// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Api.Controllers
{
    using System.Net;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Constants;
    using Fsel.Core.Base;
    using Fsel.Course.Domain.IRepositories;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Attributes;
    using Fsel.Course.Lms.Application.Commands.AiCmd;
    using Fsel.Course.Lms.Application.Commands.VideoTimeCodeAnswerCmd;

    [ApiVersions(ApiSettings.APIVersion1)]
    [Route(Settings.APIDefaultRoute + "/class-forum-tool")]
    [ApiController]
    public class ClassForumToolController : BaseController
    {
        private readonly IMediator _mediator;

        public ClassForumToolController(IMediator mediator, IClassForumResultRepository classForumResultRepository)
        {
            _mediator = mediator;
        }


        /// <summary>
        /// Update try again
        /// </summary>
        [HttpPut("tool-resent-classforum")]
        [ProducesResponseType(typeof(MethodResult<bool>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Retry([FromBody] AutoSubmitClassForumCommand command)
        {
            ArgumentNullException.ThrowIfNull(command);
            MethodResult<bool> commandResult = await _mediator.Send(command).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Update video time code
        /// </summary>
        [HttpPut("tool-update-video-time-code")]
        [ProducesResponseType(typeof(MethodResult<bool>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> ToolUpdateVideoTimeCode(ToolUpdateVideoTimeCodeCommand command)
        {
            ArgumentNullException.ThrowIfNull(command);
            MethodResult<bool> commandResult = await _mediator.Send(command).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Update video time code
        /// </summary>
        [HttpPut("tool-update-video-result")]
        [ProducesResponseType(typeof(MethodResult<bool>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> ToolUpdateVideoResultCommand(ToolUpdateVideoResultCommand command)
        {
            ArgumentNullException.ThrowIfNull(command);
            MethodResult<bool> commandResult = await _mediator.Send(command).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }
    }
}
