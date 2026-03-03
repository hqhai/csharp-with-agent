// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Api.Controllers
{
    using System.Net;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Attributes;
    using Fsel.Common.Constants;
    using Fsel.Course.Lms.Application.Commands.OtherCmd;
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
        /// send student complete course
        /// </summary>
        [HttpPost("send-student-complete-course")]
        [ProducesResponseType(typeof(MethodResult<bool>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> SendMailFinishCourse([FromBody] SendMailFinishCourseCommand command)
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

        /// <summary>
        /// send students complete course
        /// </summary>
        [HttpPost("send-student-complete-pt")]
        [ProducesResponseType(typeof(MethodResult<bool>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> SendMailFinishPT([FromBody] SendMailFinishPTCommand command)
        {
            var queryResult = await _mediator.Send(command).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Export pt to pdf
        /// </summary>
        [HttpPost("export-pt-to-pdf")]
        [ProducesResponseType(typeof(MethodResult<byte[]>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> ExportFromHtmlRaw([FromBody] ExportPlacementTestCommand command)
        {
            var commandResult = await _mediator.Send(command).ConfigureAwait(false);
            if (!commandResult.IsOK || commandResult.Result == null)
            {
                return commandResult.GetActionResult();
            }
            return File(commandResult.Result, "application / pdf", "bao_cao_ket_qua_placement_test.pdf");
        }
    }
}
