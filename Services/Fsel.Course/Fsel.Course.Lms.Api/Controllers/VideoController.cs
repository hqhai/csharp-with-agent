// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Api.Controllers
{
    using System.Net;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Constants;
    using Fsel.Shared.Enums;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Lms.Application.Commands.VideoTimeCodeAnswerCmd;
    using Fsel.Course.Lms.Application.Queries.VideoQuery;
    using MediatR;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;

    [ApiVersion(Settings.APIVersion)]
    [Route(Settings.APIDefaultRoute + "/video")]
    [ApiController]
    [Authorize(Roles = nameof(EnumRole.Student))]
    public class VideoController : ControllerBase
    {
        private readonly IMediator _mediator;

        public VideoController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Get video id
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(MethodResult<VideoModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Get([FromQuery] GetVideoQuery query)
        {
            MethodResult<VideoModel> commandResult = await _mediator.Send(query).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Create video time code answer
        /// </summary>
        [HttpPost("create-video-time-code-answer")]
        [ProducesResponseType(typeof(MethodResult<VideoTimeCodeModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> CreateVideoTimeCodeAnswer([FromBody] CreateVideoTimeCodeAnswerCommand query)
        {
            MethodResult<VideoTimeCodeModel> commandResult = await _mediator.Send(query).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }
    }
}
