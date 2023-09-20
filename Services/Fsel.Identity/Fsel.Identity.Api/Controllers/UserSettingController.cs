// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Api.Controllers
{
    using System.Net;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Constants;
    using Fsel.Identity.Application.Commands.UserSetttingCmd;
    using Fsel.Identity.Application.Queries.UserSettingQuery;
    using Fsel.Identity.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;

    [ApiVersion(Settings.APIVersion)]
    [Route(Settings.APIDefaultRoute + "/user-setting")]
    [ApiController]
    public class UserSettingController : ControllerBase
    {
        private readonly IMediator _mediator;

        public UserSettingController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Save User setting
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(MethodResult<UserSettingModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Save([FromBody] SaveUserSettingCommand command)
        {
            MethodResult<UserSettingModel> commandResult = await _mediator.Send(command).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Get User setting
        /// </summary>
        [HttpGet("get-user")]
        [ProducesResponseType(typeof(MethodResult<UserSettingModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Get()
        {
            MethodResult<UserSettingModel> commandResult = await _mediator.Send(new GetUserSettingQuery()).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }
    }
}
