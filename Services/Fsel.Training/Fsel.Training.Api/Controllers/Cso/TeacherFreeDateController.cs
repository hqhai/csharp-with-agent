// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Training.Api.Controllers.Cso
{
    using System.Collections.Generic;
    using System.Net;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Constants;
    using Fsel.Training.Application.Queries.ScheduleQuery;
    using Fsel.Training.Application.Queries.TeacherFreeDateQuery;
    using Fsel.Training.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;

    [ApiVersion(Settings.APIVersion)]
    [Route(Settings.APIDefaultRoute + "/cso/teacher-free-date")]
    [ApiController]
    public class TeacherTimeDateController : ControllerBase
    {
        private readonly IMediator _mediator;

        public TeacherTimeDateController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Get List free Teacher
        /// </summary>

        [HttpGet("get-list-teacher-free-date-by-teacherId")]
        [ProducesResponseType(typeof(MethodResult<IList<TeacherFreeDateModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetListFreeTeacher([FromQuery] GetListTeacherFreeDateByTeacherIdQuery query)
        {
            MethodResult<IList<TeacherFreeDateModel>> commandResult = await _mediator.Send(query).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Get class live priority
        /// </summary>

        [HttpGet("get-teacher-free-by-class-id")]
        [ProducesResponseType(typeof(MethodResult<TeacherFreeDateModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetListTeacherDate([FromQuery] GetTeacherLiveDateByClassIdQuery query)
        {
            MethodResult<TeacherFreeDateModel> queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Get class live priority
        /// </summary>

        [HttpGet("get-list-teacher-free-by-class-id")]
        [ProducesResponseType(typeof(MethodResult<IList<TeacherFreeDateModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetTeacherDate([FromQuery] GetListTeacherLiveDateByClassIdQuery query)
        {
            MethodResult<IList<TeacherFreeDateModel>> queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }
    }
}
