// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Api.Controllers
{
    using System.Net;
    using Asp.Versioning;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Constants;
    using Fsel.Core.Base;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Identity.Application.Commands.DailyStreakCmd;
    using Fsel.Identity.Application.Queries.DailyStreakQuery;
    using Fsel.Identity.Domain.IRepositories;
    using Fsel.Identity.Domain.Models.EntityModels;
    using Fsel.Shared.Constants;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;

    [ApiVersion(ApiSettings.APIVersion1)]
    [ApiVersion(ApiSettings.APIVersion1i1)]
    [Route(Settings.APIDefaultRoute + "/student-daily-streak")]
    [ApiController]
    public class StudentDailyStreakController : BaseController
    {
        private readonly IMediator _mediator;
        private readonly IStudentDailyStreakRepository _studentDailyStreakRepository;

        public StudentDailyStreakController(IMediator mediator, IStudentDailyStreakRepository studentDailyStreakRepository)
        {
            _mediator = mediator;
            _studentDailyStreakRepository = studentDailyStreakRepository;
        }

        /// <summary>
        /// execute list query
        /// </summary>
        [HttpGet("execute-list-query")]
        [ProducesResponseType(typeof(MethodResult<bool>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> ReceiveToken([FromQuery] BaseQueryModel query)
        {
            SetQuery(query);
            var commandResult = await _studentDailyStreakRepository.GetListResultAsync<StudentConsecutiveDayModel>(query);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Receive Token Student
        /// </summary>
        [HttpPost("receive-token")]
        [ProducesResponseType(typeof(MethodResult<double?>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> ReceiveToken([FromBody] ReceiveTokensStudentCommand command)
        {
            MethodResult<double?> commandResult = await _mediator.Send(command).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Get Consecutive Days
        /// </summary>
        [HttpGet("consecutive-days")]
        [ProducesResponseType(typeof(MethodResult<StudentDailyStreakModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetConsecutiveDays()
        {
            MethodResult<StudentDailyStreakModel> commandResult = await _mediator.Send(new GetStudentConsecutiveDaysQuery()).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Get Years
        /// </summary>
        [HttpGet("years")]
        [ProducesResponseType(typeof(MethodResult<IList<int>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetYears()
        {
            MethodResult<IList<int>> commandResult = await _mediator.Send(new GetYearDailyStreakQuery()).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Get Student Armorials
        /// </summary>
        [HttpGet("armorials")]
        [ProducesResponseType(typeof(MethodResult<IList<DateTime>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetStudentArmorials([FromQuery] GetStudentArmorialQuery query)
        {
            MethodResult<IList<DateTime>> commandResult = await _mediator.Send(query).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Get daily streak
        /// </summary>
        [HttpGet("get-daily-streak/{id}")]
        [ProducesResponseType(typeof(MethodResult<DailyStreakModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetDailyStreak([FromRoute] Guid id)
        {
            MethodResult<DailyStreakModel> commandResult = await _mediator.Send(new GetDailyStreakByUserIdQuery { Id = id }).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Create a daily streak
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(MethodResult<bool>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Create([FromBody] CreateStudentDailyStreakCommand command)
        {
            MethodResult<bool> commandResult = await _mediator.Send(command).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }
    }
}
