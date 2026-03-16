// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Api.Controllers.V1i2
{
    using System.Net;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Attributes;
    using Fsel.Common.Constants;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Lms.Application.Commands.VideoResultCmd;
    using Fsel.Shared.Attributes;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;
    using ReviewLessonVideoCommand = Application.Commands.VideoResultCmd.V1i2.ReviewLessonVideoCommand;
    using UpdateShowTokenVideoResultCommand = Application.Commands.VideoResultCmd.V1i2.UpdateShowTokenVideoResultCommand;

    [ApiVersions(ApiSettings.APIVersion1i2)]
    [Route(Settings.APIDefaultRoute + "/video-result")]
    [ApiController]
    [Permission(roles: new string[] { nameof(EnumRole.Student), nameof(EnumRole.StudentCampus) })]
    public class VideoResultController : ControllerBase
    {
        private readonly IMediator _mediator;

        public VideoResultController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Review lesson video
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(MethodResult<VideoResultModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> ReviewLessonVideo([FromBody] ReviewLessonVideoCommand command)
        {
            MethodResult<VideoResultModel> commandResult = await _mediator.Send(command).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Review lesson video
        /// </summary>
        [HttpPut("{videoResultId}")]
        [ProducesResponseType(typeof(MethodResult<bool>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> UpdateShowCoinVideo([FromRoute] Guid videoResultId)
        {
            MethodResult<bool> commandResult = await _mediator.Send(new UpdateShowTokenVideoResultCommand { VideoResultId = videoResultId }).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Review lesson video
        /// </summary>
        [HttpPut("playback-speed")]
        [ProducesResponseType(typeof(MethodResult<VideoResultModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> SetPlaybackSpeed([FromBody] UpdatePlaybackSpeedCommand command)
        {
            MethodResult<VideoResultModel> commandResult = await _mediator.Send(command).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }
    }
}
