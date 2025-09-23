// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Api.Controllers
{
    using System.Net;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Constants;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Lms.Application.Commands.VideoResultCmd;
    using Fsel.Shared.Attributes;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;

    [ApiVersions(ApiSettings.APIVersion1)]
    [Route(Settings.APIDefaultRoute + "/video-result")]
    [ApiController]
    [Common.Attributes.Permission(roles: new string[] { nameof(EnumRole.Student), nameof(EnumRole.StudentCampus) })]
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
        [HttpPut("{lessonResultId}")]
        [ProducesResponseType(typeof(MethodResult<bool>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> UpdateShowCoinVideo([FromRoute] Guid lessonResultId)
        {
            MethodResult<bool> commandResult = await _mediator.Send(new UpdateShowTokenVideoResultCommand { LessonResultId = lessonResultId }).ConfigureAwait(false);
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
