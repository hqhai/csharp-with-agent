// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Api.Controllers
{
    using System.Net;
    using Asp.Versioning;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Attributes;
    using Fsel.Common.Constants;
    using Fsel.Identity.Application.Commands.OtherCmd;
    using Fsel.Identity.Application.Commands.SystemConfigs;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;

    [ApiVersion(ApiSettings.APIVersion1)]
    [ApiVersion(ApiSettings.APIVersion1i1)]
    [Route(Settings.APIDefaultRoute + "/other")]
    [ApiController]
    public class OtherController : ControllerBase
    {
        private readonly IMediator _mediator;

        public OtherController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// RetakeCourse
        /// </summary>
        [HttpGet("retake-course")]
        [ProducesResponseType(typeof(MethodResult<string>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<RedirectResult> RetakeCourse([FromQuery] RetakeCourseResultCommand command)
        {
            var commandResult = await _mediator.Send(command).ConfigureAwait(false);
            return Redirect(commandResult.Result ?? string.Empty);
        }

        /// <summary>
        /// SystemConfig
        /// </summary>
        [HttpPost("system-config")]
        [ProducesResponseType(typeof(MethodResult<bool>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [Permission(roles: new[] { nameof(EnumRole.MasterAdmin) })]
        public async Task<IActionResult> Save([FromBody] SaveSystemConfigCommand command)
        {
            var commandResult = await _mediator.Send(command).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }
    }
}
