// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Api.Controllers
{
    using System.Net;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Constants;
    using Fsel.Identity.Application.Commands.DailyStreakCmd;
    using Fsel.Identity.Application.Queries.DailyStreakQuery;
    using Fsel.Identity.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;

    [ApiVersion(Settings.APIVersion)]
    [Route(Settings.APIDefaultRoute + "/student-daily-streak")]
    [ApiController]
    public class StudentDailyStreakController : ControllerBase
    {
        private readonly IMediator _mediator;

        public StudentDailyStreakController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Receive Tokens Student
        /// </summary>
        [HttpPost("receive-token")]
        [ProducesResponseType(typeof(MethodResult<bool>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> ReceiveToken([FromBody] Guid id)
        {
            MethodResult<bool> commandResult = await _mediator.Send(new ReceiveTokensStudentCommand { Id = id }).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Get Consecutive Days
        /// </summary>
        [HttpGet("get-consecutive")]
        [ProducesResponseType(typeof(MethodResult<StudentDailyStreakModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetConsecutiveDays()
        {
            MethodResult<StudentDailyStreakModel> commandResult = await _mediator.Send(new GetStudentConsecutiveDaysQuery()).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }
    }
}
