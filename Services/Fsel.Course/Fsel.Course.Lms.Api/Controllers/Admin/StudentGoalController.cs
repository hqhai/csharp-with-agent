// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Api.Controllers.Admin
{
    using System.Net;
    using Core.Base;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Attributes;
    using Fsel.Common.Constants;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Lms.Application.Commands.StudentGoalAggregateCmd;
    using Fsel.Course.Lms.Application.Queries.StudentAggregateQuery;
    using Fsel.Course.Lms.Application.Queries.StudentGoalSummaryQuery;
    using Fsel.Shared.Attributes;
    using Fsel.Shared.Constants;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;
    using Refit;

    [ApiVersions(ApiSettings.APIVersion1)]
    [Route(Settings.APIDefaultRoute + "/admin/student-goal")]
    [ApiController]
    public class StudentGoalController : BaseController
    {
        private readonly IMediator _mediator;

        public StudentGoalController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// delete StudentGoalAggregate
        /// </summary>
        [HttpDelete("aggregate")]
        [ProducesResponseType(typeof(MethodResult<bool>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.BadRequest)]
        [Permission(StudentManagement.Delete)]
        public async Task<IActionResult> Delete([FromBody] DeleteStudentGoalAggregateCommand command)
        {
            MethodResult<bool> commandResult = await _mediator.Send(command).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Search StudentGoalAggregate
        /// </summary>
        [HttpGet("summary")]
        [ProducesResponseType(typeof(MethodResult<PagingItemsModel<StudentGoalSummaryModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.BadRequest)]
        [Permission(permissionCodes: new string[] { StudentManagement.View, StudentCampusManagement.View })]
        public async Task<IActionResult> Get([FromQuery] SearchStudentGoalSummaryQuery query)
        {
            MethodResult<PagingItemsModel<StudentGoalSummaryModel>> commandResult = await _mediator.Send(query).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Search StudentGoalAggregate
        /// </summary>
        [HttpGet("aggregate")]
        [ProducesResponseType(typeof(MethodResult<PagingItemsModel<StudentGoalAggregateModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.BadRequest)]
        [Permission(StudentProgressWeeklyManagement.View)]
        public async Task<IActionResult> Get([FromQuery] SearchStudentGoalAggregateQuery query)
        {
            MethodResult<PagingItemsModel<StudentGoalAggregateModel>> commandResult = await _mediator.Send(query).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>s
        /// send notify after pt
        /// </summary>
        [HttpPost("send-email-weekly-progress-report")]
        [ProducesResponseType(typeof(MethodResult<bool>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> SendEmailWeeklyProgressReport([FromBody] SendEmailWeeklyProgressReportCommand command)
        {
            var queryResult = await _mediator.Send(command).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>s
        /// send notify after pt
        /// </summary>
        [HttpPost("send-email-learning-progress-warning")]
        [ProducesResponseType(typeof(MethodResult<bool>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> SendEmailLearningProgressWarning([FromBody] SendEmailLearningProgressWarningCommand command)
        {
            var queryResult = await _mediator.Send(command).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// update status student goal
        /// </summary>
        [HttpPut("update-status/{id}")]
        [ProducesResponseType(typeof(MethodResult<StausStudentGoalHistoryModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.BadRequest)]
        [Permission(StudentProgressWeeklyManagement.View)]
        public async Task<IActionResult> Get([FromRoute] Guid id, [FromBody] UpdateStudentGoalStatusCommand command)
        {
            ArgumentNullException.ThrowIfNull(command);
            command.StudentId = id;
            var commandResult = await _mediator.Send(command).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// get status history of student
        /// </summary>
        [HttpGet("status-history/{studentId}")]
        [ProducesResponseType(typeof(MethodResult<StausStudentGoalHistoryModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.BadRequest)]
        [Permission(StudentProgressWeeklyManagement.View)]
        public async Task<IActionResult> Get([FromRoute] Guid studentId)
        {
            var commandResult = await _mediator.Send(new GetStudentGoalStatusHistoryQuery {StudentId = studentId}).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Export Student Goal
        /// </summary>
        [HttpGet("export-student-goal")]
        [ProducesResponseType(typeof(MethodResult<Stream>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [Permission(StudentProgressWeeklyManagement.View)]
        public async Task<IActionResult> Get([FromQuery] ExportFileExcelStudentGoalCommand command)
        {
            SetQuery(command);
            var commandResult = await _mediator.Send(command).ConfigureAwait(false);
            if (!commandResult.IsOK || commandResult.Result == null)
            {
                return commandResult.GetActionResult();
            }

            var exportDate = DateTime.UtcNow.AddHours(7);
            string fileName = $"Tien_Do_Tuan_{exportDate:dd_MM_yyyy}.xlsx";
            return File(commandResult.Result, Settings.Excels.ContentType, fileName);
        }

        /// <summary>
        /// get status history of student
        /// </summary>
        [HttpGet("campus-code")]
        [ProducesResponseType(typeof(MethodResult<IList<string>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> Get()
        {
            var commandResult = await _mediator.Send(new GetClassCampusQuery()).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }
    }
}
