// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Api.Controllers.V1i2
{
    using System.Net;
    using Common.ActionResults;
    using Common.Attributes;
    using Common.Constants;
    using Domain.Models.EntityModels;
    using Fsel.Course.Lms.Application.Commands.VideoTimeCodeAnswerCmd;
    using Fsel.Course.Lms.Application.Commands.VideoTimeCodeResultCmd;
    using Fsel.Course.Lms.Application.Queries.VideoQuery.V1i2;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;
    using Shared.Attributes;
    using Shared.Constants;
    using Shared.Enums;

    [ApiVersions(ApiSettings.APIVersion1i2)]
    [Route(Settings.APIDefaultRoute + "/video")]
    [ApiController]
    [Permission(roles: new[] { nameof(EnumRole.Student) })]
    public class VideoController : ControllerBase
    {
        private readonly IMediator _mediator;

        public VideoController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Get Video Time Code
        /// </summary>
        [HttpGet("time-code")]
        [ProducesResponseType(typeof(MethodResult<VideoModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetTimeCode([FromQuery] GetVideoTimeCodeQuery query)
        {
            var commandResult = await _mediator.Send(query).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Get Video Time Code Detail
        /// </summary>
        [EncryptResponse]
        [HttpGet("time-code-detail")]
        [ProducesResponseType(typeof(MethodResult<VideoTimeCodeModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetTimeCodeDetail([FromQuery] GetTimeCodeDetailQuery query)
        {
            MethodResult<VideoTimeCodeModel> commandResult = await _mediator.Send(query).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Create video time code answer
        /// </summary>
        [HttpPost("create-video-time-code-answer")]
        [ProducesResponseType(typeof(MethodResult<bool>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> CreateVideoTimeCodeAnswer([FromBody] CreateVideoTimeCodeAnswerCommand query)
        {
            MethodResult<bool> commandResult = await _mediator.Send(query).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Action Video Time Code
        /// </summary>
        [HttpPost("action-time-code")]
        [ProducesResponseType(typeof(MethodResult<VideoResultModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> ActionTimeCode([FromBody] ActionVideoTimeCodeCommand command)
        {
            var queryResult = await _mediator.Send(command).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }
    }
}
