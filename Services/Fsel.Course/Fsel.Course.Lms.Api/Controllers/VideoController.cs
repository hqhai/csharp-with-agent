// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Api.Controllers
{
    using System.Net;
    using Asp.Versioning;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Attributes;
    using Fsel.Common.Constants;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Lms.Application.Commands.VideoTimeCodeAnswerCmd;
    using Fsel.Course.Lms.Application.Queries.VideoQuery;
    using Fsel.Shared.Attributes;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;

    [ApiVersions(ApiSettings.APIVersion1)]
    [Route(Settings.APIDefaultRoute + "/video")]
    [ApiController]
    [Common.Attributes.Permission(roles: new string[] { nameof(EnumRole.Student), nameof(EnumRole.StudentCampus) })]
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
        /// Get Video Time Code
        /// </summary>
        [HttpGet("time-code")]
        [MapToApiVersion(ApiSettings.APIVersion1)]
        [MapToApiVersion(ApiSettings.APIVersion1i1)]
        [ProducesResponseType(typeof(MethodResult<VideoModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetTimeCode([FromQuery] GetVideoTimeCodeQuery query)
        {
            MethodResult<VideoModel> commandResult = await _mediator.Send(query).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Get Video Time Code Detail
        /// </summary>
        [MapToApiVersion(ApiSettings.APIVersion1)]
        [MapToApiVersion(ApiSettings.APIVersion1i1)]
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
        [MapToApiVersion(ApiSettings.APIVersion1)]
        public async Task<IActionResult> CreateVideoTimeCodeAnswer([FromBody] CreateVideoTimeCodeAnswerCommand query)
        {
            MethodResult<bool> commandResult = await _mediator.Send(query).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }
    }
}
