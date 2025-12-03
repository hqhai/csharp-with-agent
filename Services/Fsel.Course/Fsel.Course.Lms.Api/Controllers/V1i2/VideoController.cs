// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Api.Controllers.V1i2
{
    using System.Net;
    using Application.Queries.VideoQuery;
    using Common.ActionResults;
    using Common.Attributes;
    using Common.Constants;
    using Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;
    using Shared.Attributes;
    using Shared.Constants;
    using Shared.Enums;
    using GetVideoTimeCodeQuery = Application.Queries.VideoQuery.V1i2.GetVideoTimeCodeQuery;

    [ApiVersions(ApiSettings.APIVersion1i2)]
    [Route(Settings.APIDefaultRoute + "/video")]
    [ApiController]
    [Permission(role: nameof(EnumRole.Student))]
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
        //[EncryptResponse]
        [HttpGet("time-code-detail")]
        [ProducesResponseType(typeof(MethodResult<VideoTimeCodeModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetTimeCodeDetail([FromQuery] GetTimeCodeDetailQuery query)
        {
            MethodResult<VideoTimeCodeModel> commandResult = await _mediator.Send(query).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }
    }
}
