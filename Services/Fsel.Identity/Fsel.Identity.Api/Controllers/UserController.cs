// Copyright (c) Atlantic. All rights reserved.

using System.Net;
using Fsel.Common.ActionResults;
using Fsel.Common.Constants;
using Fsel.Identity.Application.Commands.AuthCmd;
using Fsel.Identity.Application.Commands.UserCmd;
using Fsel.Identity.Application.Queries.UserQuery;
using Fsel.Identity.Domain.Models.EntityModels;
using Fsel.Shared.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Fsel.Identity.Api.Controllers
{
    [ApiVersion(Settings.APIVersion)]
    [Route(Settings.APIDefaultRoute + "/user")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IMediator _mediator;

        public UserController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Change Password
        /// </summary>
        [HttpPost("change-password")]
        [ProducesResponseType(typeof(MethodResult<bool>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [Authorize]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordCommand command)
        {
            MethodResult<bool> commandResult = await _mediator.Send(command).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Update profile user
        /// </summary>
        [Authorize]
        [HttpPut("update-profile-user")]
        [ProducesResponseType(typeof(MethodResult<UserModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> UpdateProfileUser([FromBody] UpdateUserProfileCommand command)
        {
            MethodResult<UserModel> commandResult = await _mediator.Send(command).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Update Code Student
        /// </summary>
        [HttpPut("update-code-student")]
        [ProducesResponseType(typeof(MethodResult<UserModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> UpdateCodeStudent([FromBody] UpdateCodeStudentCommand command)
        {
            MethodResult<UserModel> commandResult = await _mediator.Send(command).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Get User Profile
        /// </summary>
        [Authorize]
        [HttpGet("get-user-profile")]
        [ProducesResponseType(typeof(MethodResult<UserProfileModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetProfileUser()
        {
            MethodResult<UserProfileModel> commandResult = await _mediator.Send(new GetUserProfileQuery()).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Get users by role
        /// </summary>
        [HttpGet("get-users-by-role")]
        [ProducesResponseType(typeof(MethodResult<IList<UserModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetUsersByRole([FromQuery] GetUsersByRoleQuery query)
        {
            MethodResult<IList<UserModel>> commandResult = await _mediator.Send(query).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Get users by ids
        /// </summary>
        [HttpPost("get-users-by-ids")]
        [ProducesResponseType(typeof(MethodResult<IList<HumanModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetUsersByIds([FromBody] GetUsersByIdsQuery query)
        {
            MethodResult<IList<HumanModel>> commandResult = await _mediator.Send(query).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Get users by ids
        /// </summary>
        [HttpGet("get-user-by-id")]
        [ProducesResponseType(typeof(MethodResult<HumanModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetUsersByIds([FromQuery] GetUserByIdQuery query)
        {
            MethodResult<HumanModel> commandResult = await _mediator.Send(query).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }
    }
}
