// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Training.Api.Controllers.Cso
{
    using System.Net;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Constants;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Training.Application.Commands.ClassCmd;
    using Fsel.Training.Application.Queries.ClassQuery;
    using Fsel.Training.Application.Queries.Cso;
    using Fsel.Training.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;

    [ApiVersion(Settings.APIVersion)]
    [Route(Settings.APIDefaultRoute + "/cso/class-live")]
    [ApiController]
    public class ClassLiveController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ClassLiveController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Search  class live by cso
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(MethodResult<PagingItemsModel<ClassLiveCalendarModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Search([FromQuery] SearchClassLiveByCsoQuery query)
        {
            MethodResult<PagingItemsModel<ClassLiveCalendarModel>> queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Get class live
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(MethodResult<ClassLiveCalendarModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Get([FromRoute] Guid id)
        {
            MethodResult<ClassLiveCalendarModel> commandResult = await _mediator.Send(new GetClassLiveByCsoQuery { Id = id }).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Update a class live
        /// </summary>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(MethodResult<ClassLiveCalendarModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] SaveClassLiveCsoCommand command)
        {
            ArgumentNullException.ThrowIfNull(command);
            command.Id = id;
            MethodResult<ClassLiveCalendarModel> commandResult = await _mediator.Send(command).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Get list class live
        /// </summary>

        [HttpGet("list-class-live")]
        [ProducesResponseType(typeof(MethodResult<IList<ClassModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetListClassLive([FromQuery] GetListClassLiveManageQuery query)
        {
            MethodResult<IList<ClassModel>> queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }
    }
}
