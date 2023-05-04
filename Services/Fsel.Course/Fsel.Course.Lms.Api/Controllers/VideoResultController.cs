// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Api.Controllers
{
    using System.Net;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Constants;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Lms.Application.Commands.VideoResultCmd;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;

    [ApiVersion(Settings.APIVersion)]
    [Route(Settings.APIDefaultRoute + "/video-result")]
    [ApiController]
    [Authorize(Roles = nameof(EnumRole.Student))]
    public class VideoResultController : ControllerBase
    {
        private readonly IMediator _mediator;

        public VideoResultController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// review lesson video
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(MethodResult<VideoResultModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> ReviewLessonVideo([FromBody] ReviewLessonVideoCommand command)
        {
            MethodResult<VideoResultModel> commandResult = await _mediator.Send(command).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }
    }
}
