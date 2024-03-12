// Copyright (c) Atlantic. All rights reserved.

using System.Net;
using Asp.Versioning;
using Fsel.Common.ActionResults;
using Fsel.Common.Constants;
using Fsel.Identity.Application.Commands.StudentRegistrationCmd;
using Fsel.Identity.Application.Queries.StudentTrialRegistrationQuery;
using Fsel.Identity.Domain.Entities;
using Fsel.Shared.Constants;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Fsel.Identity.Api.Controllers
{
    [ApiVersion(ApiSettings.APIVersion1)]
    [ApiVersion(ApiSettings.APIVersion1i1)]
    [Route(Settings.APIDefaultRoute + "/student-trial-registration")]
    [ApiController]
    public class StudentTrialRegistrationController : ControllerBase
    {
        private readonly IMediator _mediator;

        public StudentTrialRegistrationController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Lưu thông tin đăng kí học thử
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(MethodResult<StudentTrialRegistration>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> CreateStudentTrialRegistration()
        {
            MethodResult<StudentTrialRegistration> commandResult = await _mediator.Send(new CreateStudentTrialRegistrationCommand()).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }


        /// <summary>
        /// Check học sinh có đang học thử không
        /// </summary>
        /// <param name="studentId"></param>
        /// <returns></returns>
        [HttpGet("check")]
        [ProducesResponseType(typeof(MethodResult<bool>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> CheckStudentRegistration()
        {
            MethodResult<bool> commandResult = await _mediator.Send(new CheckStudentTrialRegistrationQuery()).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Lấy thông tin học sinh học thử
        /// </summary>
        /// <param name="studentId"></param>
        /// <returns></returns>
        [HttpGet("userId")]
        [ProducesResponseType(typeof(MethodResult<StudentTrialRegistration>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetStudentRegistration([FromRoute] Guid userId)
        {
            MethodResult<StudentTrialRegistration> commandResult = await _mediator.Send(new GetStudentTrialRegistrationQuery { UserId = userId }).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }


        /// <summary>
        /// Lưu thông tin đăng kí học thử
        /// </summary>
        [HttpPut]
        [ProducesResponseType(typeof(MethodResult<StudentTrialRegistration>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> UpdateStudentTrialRegistration([FromBody] UpdateStudentTrialRegistrationCommand cmd)
        {
            MethodResult<bool> commandResult = await _mediator.Send(cmd).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }
    }
}
