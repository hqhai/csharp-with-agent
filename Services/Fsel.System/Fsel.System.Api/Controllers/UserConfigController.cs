// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Api.Controllers
{
    using Asp.Versioning;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Attributes;
    using Fsel.Common.Constants;
    using Fsel.Shared.Constants;
    using Fsel.System.Application.Commands.UserConfigs;
    using Fsel.System.Application.Queries.UserConfigs;
    using global::System.Net;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;

    [ApiVersion(ApiSettings.APIVersion1)]
    [ApiVersion(ApiSettings.APIVersion1i1)]
    [Route(Settings.APIDefaultRoute + "/user-config")]
    [ApiController]
    [Permission]
    public class UserConfigController : ControllerBase
    {
        private readonly IMediator _mediator;

        public UserConfigController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Get is view new features
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(MethodResult<bool>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Get()
        {
            var commandResult = await _mediator.Send(new GetViewNewFeaturesByUserQuery()).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Create user config
        /// </summary>
        [HttpPost()]
        [ProducesResponseType(typeof(MethodResult<bool>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Create()
        {
            var commandResult = await _mediator.Send(new CreateUserConfigCommand()).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }
    }
}
