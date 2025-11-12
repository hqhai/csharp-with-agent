// Copyright (c) Atlantic. All rights reserved.

using System.Net;
using Asp.Versioning;
using Fsel.Common.ActionResults;
using Fsel.Common.Attributes;
using Fsel.Common.Constants;
using Fsel.Identity.Application.Commands.AuthCmd;
using Fsel.Identity.Application.Commands.LandingPages;
using Fsel.Identity.Application.Commands.UserCmd;
using Fsel.Identity.Application.Commands.UserReferrals;
using Fsel.Identity.Application.Queries.UserQuery;
using Fsel.Identity.Application.Queries.UserReferrals;
using Fsel.Identity.Domain.Models.EntityModels;
using Fsel.Shared.Constants;
using Fsel.Shared.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Fsel.Identity.Application.Queries.AuthQuery;
using Fsel.Core.Applications.Attributes;

namespace Fsel.Identity.Api.Controllers
{
    [ApiVersion(ApiSettings.APIVersion1)]
    [ApiVersion(ApiSettings.APIVersion1i1)]
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
        [Common.Attributes.Permission]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordCommand command)
        {
            MethodResult<bool> commandResult = await _mediator.Send(command).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Update profile user
        /// </summary>
        [Common.Attributes.Permission]
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
        [TenantAware]
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
        [Common.Attributes.Permission]
        [HttpGet("get-user-profile")]
        [ProducesResponseType(typeof(MethodResult<UserProfileModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [ServerCache(CacheSettings.TimeCache.OneMinutes)]
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
        [ProducesResponseType(typeof(MethodResult<IList<UserModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetUsersByIds([FromBody] GetUsersByIdsQuery query)
        {
            MethodResult<IList<UserModel>> commandResult = await _mediator.Send(query).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Get users by ids
        /// </summary>
        [HttpGet("get-user-by-id")]
        [ProducesResponseType(typeof(MethodResult<UserModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetUsersByIds([FromQuery] GetUserByIdQuery query)
        {
            MethodResult<UserModel> commandResult = await _mediator.Send(query).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Get users by ids
        /// </summary>
        [HttpPost("get-users-by-userids")]
        [ProducesResponseType(typeof(MethodResult<IList<UserModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetUsersByUserIds([FromBody] IList<Guid>? userIds)
        {
            MethodResult<IList<UserModel>> commandResult = await _mediator.Send(new GetUsersByUserIdsQuery { UserIds = userIds }).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Get info student or guest by student id
        /// </summary>
        [HttpGet("get-by-student-id/{id}")]
        [ProducesResponseType(typeof(MethodResult<StudentModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetInfoStudentOrGuest([FromRoute] Guid id)
        {
            MethodResult<StudentModel> commandResult = await _mediator.Send(new GetInfoStudentOrGuestByStudentIdQuery { StudentId = id }).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Delete
        /// </summary>
        [HttpDelete]
        [ProducesResponseType(typeof(MethodResult<bool>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [Common.Attributes.Permission]
        public async Task<IActionResult> DeleteUser()
        {
            MethodResult<bool> commandResult = await _mediator.Send(new DeleteUserCommand()).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// receive data from landing page
        /// </summary>
        [HttpPost("receive-data-from-landing-page")]
        [ProducesResponseType(typeof(MethodResult<bool>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> ReceiveDataFromLandingPage([FromBody] ReceiveDataFromLandingPageCommand command)
        {
            var commandResult = await _mediator.Send(command).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// create user referral
        /// </summary>
        [HttpPost("create-user-referral")]
        [ProducesResponseType(typeof(MethodResult<VoidMethodResult>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> CreateUserReferral([FromBody] CreateUserReferralCommand command)
        {
            var commandResult = await _mediator.Send(command).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// get user referrals
        /// </summary>
        [HttpGet("get-user-referrals")]
        [Common.Attributes.Permission(roles: new string[] { nameof(EnumRole.Student), nameof(EnumRole.StudentCampus) })]
        [ProducesResponseType(typeof(MethodResult<UserReferralsModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetUserReferral([FromQuery] GetUserReferralsByUserQuery query)
        {
            var commandResult = await _mediator.Send(query).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// check user referral code
        /// </summary>
        [HttpGet("check-user-referral-code")]
        [ProducesResponseType(typeof(MethodResult<SenderModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> CheckUserReferralCode([FromQuery] CheckReferralCodeQuery query)
        {
            var commandResult = await _mediator.Send(query).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// check user referral code
        /// </summary>
        [HttpGet("get-sender-by-code")]
        [Common.Attributes.Permission(roles: new string[] { nameof(EnumRole.Student), nameof(EnumRole.StudentCampus) })]
        [ProducesResponseType(typeof(MethodResult<SenderModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetSenderByCode([FromQuery] GetSenderByCodeQuery query)
        {
            var commandResult = await _mediator.Send(query).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Tool get otp of student
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        [HttpGet("tool-get-otp-for-user")]
        [ProducesResponseType(typeof(MethodResult<UserOtpCodeModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> ToolGetOtp([FromQuery] ToolGetOtpQuery query)
        {
            MethodResult<UserOtpCodeModel> commandResult = await _mediator.Send(query).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Disconnect external connect
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        [HttpPost("disconnect-external-provider")]
        [Common.Attributes.Permission]
        [ProducesResponseType(typeof(MethodResult<bool>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> DisconnectExternalProvider([FromBody] DisconnectExternalAccountCommand command)
        {
            ArgumentException.ThrowIfNullOrEmpty(command?.Provider);
            var commandResult = await _mediator.Send(command).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Get external provider connects
        /// </summary>
        /// <returns></returns>
        [HttpGet("get-external-provider-connects")]
        [Common.Attributes.Permission]
        [ProducesResponseType(typeof(MethodResult<bool>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetExternalProviderConnects([FromQuery] GetExternalConnectsQuery query)
        {
            var result = await _mediator.Send(query).ConfigureAwait(false);
            return result.GetActionResult();
        }

        /// <summary>
        /// get role by user ud
        /// </summary>
        [HttpGet("get-role-by-user-id/{userId}")]
        [ProducesResponseType(typeof(MethodResult<string?>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [Permission]
        [ServerCache(CacheSettings.TimeCache.OneHour)]
        public async Task<IActionResult> GetRoleByUserId([FromRoute] string userId)
        {
            var commandResult = await _mediator.Send(new GetRoleByUserIdQuery() { UserId = userId }).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }
    }
}
