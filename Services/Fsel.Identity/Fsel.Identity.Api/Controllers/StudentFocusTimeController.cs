// Copyright (c) Atlantic. All rights reserved.

using System.Net;
using Asp.Versioning;
using Fsel.Common.ActionResults;
using Fsel.Common.Attributes;
using Fsel.Common.Constants;
using Fsel.Identity.Application.Commands.StudentFocusTimeCmd;
using Fsel.Identity.Application.Queries.StudentFocusTimeQuery;
using Fsel.Identity.Domain.Models.EntityModels;
using Fsel.Shared.Constants;
using Fsel.Shared.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Fsel.Identity.Api.Controllers
{
    [ApiVersion(ApiSettings.APIVersion1)]
    [ApiVersion(ApiSettings.APIVersion1i1)]
    [Route(Settings.APIDefaultRoute + "/student-focus-time")]
    [ApiController]
    public class StudentFocusTimeController : ControllerBase
    {
        private readonly IMediator _mediator;

        public StudentFocusTimeController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Lưu thông tin truy cập.
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(MethodResult<List<StudentRankingModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> CreateStudentFocusTime([FromBody] CreateStudentFocusTimeCommand cmd)
        {
            MethodResult<StudentFocusTimeModel> commandResult = await _mediator.Send(cmd).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        [HttpGet]
        [ProducesResponseType(typeof(MethodResult<StudentFocusTimeModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetStudentFocusTime()
        {
            MethodResult<StudentFocusTimeModel> commandResult = await _mediator.Send(new GetStudentFocusTimeQuery()).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        [HttpGet("check-super-fire")]
        [ProducesResponseType(typeof(MethodResult<bool>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> CheckSuperFireMode()
        {
            MethodResult<bool> commandResult = await _mediator.Send(new CheckSuperFireModeQuery()).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        [HttpPost("receive-token")]
       [Common.Attributes.Permission(roles: new string[] { nameof(EnumRole.Student), nameof(EnumRole.StudentCampus) })]
        [ProducesResponseType(typeof(MethodResult<StudentFocusTimeModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> ReceiveToken()
        {
            MethodResult<StudentFocusTimeModel> commandResult = await _mediator.Send(new CreateTokenFocusTimeCommand()).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }
    }
}
