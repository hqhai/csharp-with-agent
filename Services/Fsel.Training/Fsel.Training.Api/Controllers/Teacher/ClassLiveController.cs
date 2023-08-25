// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Training.Api.Controllers.Teacher
{
    using System.Net;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Constants;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Shared.Enums;
    using Fsel.Training.Application.Commands.ClassLiveCmd;
    using Fsel.Training.Application.Commands.ClassLiveWorkFlowCmd;
    using Fsel.Training.Application.Queries.CalendarQuery;
    using Fsel.Training.Application.Queries.ClassLiveQuery;
    using Fsel.Training.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;

    [ApiVersion(Settings.APIVersion)]
    [Route(Settings.APIDefaultRoute + "/teacher/class-live")]
    [ApiController]
    [Authorize(Roles = nameof(EnumRole.Teacher))]
    public class ClassLiveController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ClassLiveController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Create ClassLiveWorkFlow
        /// </summary>
        [HttpPost("teacher-request")]
        [ProducesResponseType(typeof(MethodResult<ClassLiveWorkFlowModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> TeacherRequest([FromBody] CreateClassLiveWorkFlowCommand command)
        {
            var queryResult = await _mediator.Send(command).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Approve teacher
        /// </summary>
        [HttpPut("approve/{id}")]
        [ProducesResponseType(typeof(MethodResult<bool>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Approve([FromRoute] Guid id, [FromBody] ApproveClassLiveCommand command)
        {
            ArgumentNullException.ThrowIfNull(command);
            command.Id = id;
            var queryResult = await _mediator.Send(command).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Approve teacher
        /// </summary>
        [HttpPut("approve-auto")]
        [ProducesResponseType(typeof(MethodResult<bool>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> ApproveAuto()
        {
            var queryResult = await _mediator.Send(new UpdateClassLiveAssignmentCommand()).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        ///  <summary>
        ///  Search ClassLive
        ///  </summary>
        [HttpGet("assignments")]
        [ProducesResponseType(typeof(MethodResult<PagingItemsModel<ClassLiveModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Search([FromQuery] SearchClassLiveAssignmentsQuery query)
        {
            var queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Search ClassLiveCalendar
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(MethodResult<PagingItemsModel<ClassLiveCalendarSearchModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> SearchClassLiveCalendar([FromQuery] SearchClassLiveCalendarQuery query)
        {
            var queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }
    }
}
