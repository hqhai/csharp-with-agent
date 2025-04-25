// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Api.Controllers
{
    using System.Net;
    using Asp.Versioning;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Attributes;
    using Fsel.Common.Constants;
    using Fsel.Identity.Application.Commands.StudentCmd;
    using Fsel.Identity.Application.Commands.UserOtpCodeCmd;
    using Fsel.Identity.Application.Queries.StudentQuery;
    using Fsel.Identity.Domain.Models.CommandModels.UserOtpCodes;
    using Fsel.Identity.Domain.Models.EntityModels;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;

    [ApiVersion(ApiSettings.APIVersion1)]
    [ApiVersion(ApiSettings.APIVersion1i1)]
    [Route(Settings.APIDefaultRoute + "/public")]
    [ApiController]
    [Permission(roles: new string[] { nameof(EnumRole.Admin) })]
    public class PublicController : ControllerBase
    {
        private readonly IMediator _mediator;

        public PublicController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// send otp
        /// </summary>
        [HttpPost("send-otp")]
        [ProducesResponseType(typeof(MethodResult<SaveOTPForUserEventHaNoiCommandModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> SendOTP([FromBody] SaveOTPForUserEventHaNoiCommand command)
        {
            var commandResult = await _mediator.Send(command).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// send otp
        /// </summary>
        [HttpPost("verify-otp")]
        [ProducesResponseType(typeof(MethodResult<bool>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> VerifyOTP([FromBody] VerifyOTPForUserEventHaNoiCommand command)
        {
            var commandResult = await _mediator.Send(command).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// update password
        /// </summary>
        [HttpPost("update-password")]
        [ProducesResponseType(typeof(MethodResult<bool>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> UpdatePassword([FromBody] UpdatePasswordForUserEventHaNoiCommand command)
        {
            var commandResult = await _mediator.Send(command).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// get info by phone number
        /// </summary>
        [HttpGet("get-info-by-phone-number")]
        [ProducesResponseType(typeof(MethodResult<StudentModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetInfo([FromQuery] GetStudentByEmailQuery query)
        {
            var commandResult = await _mediator.Send(query).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// check trường đã import chưa
        /// </summary>
        [HttpPost("check-school-import")]
        [ProducesResponseType(typeof(MethodResult<bool>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> CheckSchoolImportHistory([FromQuery] CheckSchoolImportHistoryCommand comand)
        {
            MethodResult<bool> commandResult = await _mediator.Send(comand).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// get users by phone number
        /// </summary>
        [HttpGet("get-users-by-phone-number")]
        [ProducesResponseType(typeof(MethodResult<IList<StudentModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetUsersByPhoneNumber([FromQuery] GetUsersByPhoneNumberQuery query)
        {
            var commandResult = await _mediator.Send(query).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// verify user event ha noi
        /// </summary>
        [HttpPost("verify-user-event-ha-noi")]
        [ProducesResponseType(typeof(MethodResult<bool>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> VerifyUserEventHaNoi([FromBody] VerifyUserEventHaNoiCommand command)
        {
            var commandResult = await _mediator.Send(command).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }
    }
}
