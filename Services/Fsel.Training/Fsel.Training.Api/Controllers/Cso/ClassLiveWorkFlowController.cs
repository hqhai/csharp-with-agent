// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Training.Api.Controllers.Cso
{
    using System.Net;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Constants;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Training.Application.Commands.ClassCmd;
    using Fsel.Training.Application.Queries.ClassLiveWorkFlowQuery;
    using Fsel.Training.Application.Queries.ScheduleQuery;
    using Fsel.Training.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;

    [ApiVersion(Settings.APIVersion)]
    [Route(Settings.APIDefaultRoute + "/cso/work-flow")]
    [ApiController]
    public class ClassLiveWorkFlowController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ClassLiveWorkFlowController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// search alternative calendar
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(MethodResult<PagingItemsModel<AlternativeCalendarModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Search([FromQuery] SearchAlternativeCalendarsByCsoQuery query)
        {
            MethodResult<PagingItemsModel<AlternativeCalendarModel>> queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Replace Teacher by Cso.
        /// </summary>
        [HttpPut("asign-new-teacher")]
        [ProducesResponseType(typeof(MethodResult<bool>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> ReplaceTeacher([FromBody] ReplaceTeacherByCsoCommand command)
        {
            ArgumentNullException.ThrowIfNull(command);
            MethodResult<bool> commandResult = await _mediator.Send(command).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }


        /// <summary>
        /// Get List free Teacher
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        [HttpGet("free-teacher/{id}")]
        [ProducesResponseType(typeof(MethodResult<IList<TeacherFreeDateModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetListFreeTeacher([FromRoute] Guid id)
        {
            MethodResult<IList<TeacherFreeDateModel>> commandResult = await _mediator.Send(new GetListFreeTeacherQuery { ClassLiveCalendarId = id }).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

    }
}
