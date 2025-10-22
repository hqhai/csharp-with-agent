// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Api.Controllers.Admin
{
    using System.Net;
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

    [ApiVersions(ApiSettings.APIVersion1)]
    [Route(Settings.APIDefaultRoute + "/admin/student-goal")]
    [ApiController]
    public class StudentGoalController : ControllerBase
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
        [Permission(StudentManagement.View)]
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
    }
}
