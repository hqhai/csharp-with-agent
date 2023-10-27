// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Training.Api.Controllers.Cso
{
    using System.Net;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Constants;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Shared.Enums;
    using Fsel.Training.Application.Commands.CancelScheduleLiveCmd;
    using Fsel.Training.Application.Queries.CancelScheduleLiveQuery;
    using Fsel.Training.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;

    [ApiVersion(Settings.APIVersion)]
    [Route(Settings.APIDefaultRoute + "/cso/cancel-schedule")]
    [ApiController]
    [Common.Attributes.Permission(role: nameof(EnumRole.CSO))]
    public class CancelScheduleLiveController : ControllerBase
    {
        private readonly IMediator _mediator;

        public CancelScheduleLiveController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Search Cancel Schedule Live
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(MethodResult<PagingItemsModel<CancelScheduleLiveModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Search([FromQuery] SearchCancelScheduleLiveCSOQuery query)
        {
            MethodResult<PagingItemsModel<CancelScheduleLiveModel>> queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Get Cancel Schedule Live
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(MethodResult<CancelScheduleLiveInfoModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Get([FromRoute] Guid id)
        {
            MethodResult<CancelScheduleLiveInfoModel> queryResult = await _mediator.Send(new GetCancelScheduleLiveQuery { Id = id }).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Reference Calendar
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(MethodResult<ClassLiveWorkFlowModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> ReferenceCalendar([FromBody] ReferenceCalendarCommand command)
        {
            MethodResult<ClassLiveWorkFlowModel> queryResult = await _mediator.Send(command).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }
    }
}
