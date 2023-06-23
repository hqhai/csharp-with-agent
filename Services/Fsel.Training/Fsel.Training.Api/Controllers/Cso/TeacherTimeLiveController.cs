// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Training.Api.Controllers.Cso
{
    using Fsel.Common.ActionResults;
    using System.Net;
    using Fsel.Common.Constants;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Training.Application.Queries.Schedule;
    using Fsel.Training.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;

    [ApiVersion(Settings.APIVersion)]
    [Route(Settings.APIDefaultRoute + "/cso/teacher-live-time")]
    [ApiController]
    public class TeacherTimeLiveController : ControllerBase
    {
        private readonly IMediator _mediator;

        public TeacherTimeLiveController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Get lít teacher live time
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(MethodResult<PagingItemsModel<TeacherFreeDateModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Get()
        {
            MethodResult<PagingItemsModel<TeacherFreeDateModel>> commandResult = await _mediator.Send(new SearchTeacherFreeDateByCsoQuery { }).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Get class live priority
        /// </summary>

        [HttpGet]
        [ProducesResponseType(typeof(MethodResult<PagingItemsModel<TeacherFreeTimeModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> SearchClassLive([FromQuery] SearchTeacherFreeTimeByCsoQuery query)
        {
            MethodResult<PagingItemsModel<TeacherFreeTimeModel>> queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }
    }
}
