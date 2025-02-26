// Copyright (c) Atlantic. All rights reserved.

using System.Net;
using Asp.Versioning;
using Fsel.Common.ActionResults;
using Fsel.Common.Attributes;
using Fsel.Common.Constants;
using Fsel.Identity.Application.Commands.AuthCmd;
using Fsel.Identity.Application.Commands.LandingPages;
using Fsel.Identity.Application.Commands.UserOtpCodeCmd;
using Fsel.Identity.Application.Queries.AuthQuery;
using Fsel.Identity.Domain.Models.EntityModels;
using Fsel.Shared.Constants;
using Fsel.Shared.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Fsel.Identity.Authentication.Controllers
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
        /// Sign Up
        /// </summary>
        [HttpPost("sign-up")]
        [ProducesResponseType(typeof(MethodResult<UserModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> SignUp([FromBody] SignUpCommand command)
        {
            MethodResult<UserModel> commandResult = await _mediator.Send(command).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Confirm OTP SignUp
        /// </summary>
        [HttpPost("confirm-otp-sign-up")]
        [ProducesResponseType(typeof(MethodResult<ConfirmOtpModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> ConfirmOTPSignUp([FromBody] ConfirmOtpSignUpCommand command)
        {
            MethodResult<ConfirmOtpModel> commandResult = await _mediator.Send(command).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Forgot Password
        /// </summary>
        [HttpPost("forgot-password")]
        [ProducesResponseType(typeof(MethodResult<ForgotPasswordResultModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordCommand command)
        {
            MethodResult<ForgotPasswordResultModel> commandResult = await _mediator.Send(command).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Com-firm OTP Reset Password
        /// </summary>
        [HttpPost("confirm-otp-reset-password")]
        [ProducesResponseType(typeof(MethodResult<bool>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> ConfirmOTPResetPassword([FromBody] ConfirmOtpResetPasswordCommand command)
        {
            MethodResult<bool> commandResult = await _mediator.Send(command).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Send otp email user
        /// </summary>
        [HttpPost("send-otp-profile")]
        [ProducesResponseType(typeof(MethodResult<bool>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> SendOtpProfile([FromBody] SendOtpProfileCommand command)
        {
            MethodResult<bool> commandResult = await _mediator.Send(command).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Check Otp
        /// </summary>
        [HttpPost("check-otp")]
        [ProducesResponseType(typeof(MethodResult<bool>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> CheckOtp([FromBody] CheckOtpCommand command)
        {
            MethodResult<bool> commandResult = await _mediator.Send(command).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Confirm Otp Profile
        /// </summary>
        [HttpPost("confirm-otp-profile")]
        [ProducesResponseType(typeof(MethodResult<bool>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> ConfirmOtpProfile([FromBody] ConfirmOtpProfileCommand command)
        {
            MethodResult<bool> commandResult = await _mediator.Send(command).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Sign Up as Guest
        /// </summary>
        [HttpPost("sign-up-as-guest")]
        [ProducesResponseType(typeof(MethodResult<TokenModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [Permission(roles: new string[] { nameof(EnumRole.Student) })]
        public async Task<IActionResult> SignUpAsGuest([FromBody] CreateGuestAccountCommand command)
        {
            MethodResult<TokenModel> commandResult = await _mediator.Send(command).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Check Current Password
        /// </summary>
        [HttpGet("check-current-password")]
        [ProducesResponseType(typeof(MethodResult<bool>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> CheckCurrentPassword([FromQuery] CheckCurrentPasswordQuery command)
        {
            MethodResult<bool> commandResult = await _mediator.Send(command).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Sign Up
        /// </summary>
        [HttpPost("create-account-from-landing-page")]
        [ProducesResponseType(typeof(MethodResult<bool>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> CreateAccountFromLandingPage([FromBody] CreateAccountFromLandingPageCommand command)
        {
            var commandResult = await _mediator.Send(command).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }
    }
}
