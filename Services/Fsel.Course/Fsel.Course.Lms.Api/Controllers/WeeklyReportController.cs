// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Api.Controllers
{
    using System.Net;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Attributes;
    using Fsel.Common.Constants;
    using Fsel.Course.Lms.Application.Commands.WeeklyReportCommand;
    using Fsel.Course.Lms.Application.Queries.WeeklyReportQuery;
    using Fsel.Shared.Attributes;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;

    [ApiVersions(ApiSettings.APIVersion1)]
    [Route(Settings.APIDefaultRoute + "/weekly-report")]
    [ApiController]
    [Permission(role: nameof(EnumRole.Admin))]
    public class WeeklyReportController : ControllerBase
    {
        private readonly IMediator _mediator;

        public WeeklyReportController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// weekly report
        /// </summary>
        [HttpPost("send-weekly-report")]
        [ProducesResponseType(typeof(MethodResult<bool>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> AggregateDataWeeklyReport()
        {
            var queryResult = await _mediator.Send(new SendWeeklyReportCommand()).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// weekly report
        /// </summary>
        [HttpPost("aggregate-data-weekly-report")]
        [ProducesResponseType(typeof(MethodResult<bool>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> WeeklyReport([FromBody] AggregateDataWeeklyReportCommand command)
        {
            var queryResult = await _mediator.Send(command).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// get video time code ranking
        /// </summary>
        [HttpPost("send-complete-unit")]
        [ProducesResponseType(typeof(MethodResult<bool>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> CompleteUnit([FromBody] SendStudentCompleteUnitCommand query)
        {
            var queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// send students complete mid course
        /// </summary>
        [HttpPost("send-students-complete-mid-course")]
        [ProducesResponseType(typeof(MethodResult<bool>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> CompleteMidCourse([FromBody] SendStudentCompleteMidCourseCommand command)
        {
            var queryResult = await _mediator.Send(command).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// send students complete course
        /// </summary>
        [HttpPost("send-students-complete-course")]
        [ProducesResponseType(typeof(MethodResult<bool>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> CompleteCourse([FromBody] SendMailStudentFinishCourseCommand command)
        {
            var queryResult = await _mediator.Send(command).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// send mail reminder to do pt
        /// </summary>
        [HttpPost("send-mail-reminder-pt-and-kickoff-event")]
        [ProducesResponseType(typeof(MethodResult<bool>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> SendMailReminderToDoPT([FromForm] SendMailReminderPTAndKickOffEventCommand command)
        {
            var queryResult = await _mediator.Send(command).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }
    }
}
