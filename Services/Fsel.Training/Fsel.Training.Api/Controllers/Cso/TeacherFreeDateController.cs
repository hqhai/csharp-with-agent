// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Training.Api.Controllers.Cso
{
    using Fsel.Common.ActionResults;
    using System.Net;
    using Fsel.Common.Constants;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Training.Application.Queries.ScheduleQuery;
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
        /// Get class live priority
        /// </summary>

        [HttpGet]
        [ProducesResponseType(typeof(MethodResult<IList<TeacherFreeTimeModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> SearchClassLive([FromQuery] SearchTeacherFreeDateByCsoQuery query)
        {
            MethodResult<IList<TeacherFreeTimeModel>> queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }
    }
}
