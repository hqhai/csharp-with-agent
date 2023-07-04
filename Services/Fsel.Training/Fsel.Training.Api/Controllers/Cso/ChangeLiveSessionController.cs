// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Training.Api.Controllers.Cso
{
    using System.Net;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Constants;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Shared.Enums;
    using Fsel.Training.Application.Commands.ChangeLiveSessionCmd;
    using Fsel.Training.Application.Queries.ChangeLiveSessionQuery;
    using Fsel.Training.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;

    [ApiVersion(Settings.APIVersion)]
    [Route(Settings.APIDefaultRoute + "/cso/change-live")]
    [ApiController]
    [Authorize(Roles = nameof(EnumRole.CSO))]
    public class ChangeLiveSessionController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ChangeLiveSessionController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Search Change Live Session
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(MethodResult<PagingItemsModel<ChangeLiveSessionModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Search([FromQuery] SearchChangeLiveSessionCSOQuery query)
        {
            MethodResult<PagingItemsModel<ChangeLiveSessionModel>> queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Get Change Live Session
        /// </summary>
        [HttpGet("id")]
        [ProducesResponseType(typeof(MethodResult<ChangeLiveSessionInfoModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Get([FromRoute] Guid id)
        {
            MethodResult<ChangeLiveSessionInfoModel> queryResult = await _mediator.Send(new GetChangeLiveSessionQuery { Id = id }).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Reference Calendar
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(MethodResult<ClassLiveWorkFlowPlanModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> ReferenceCalendar([FromBody] ReferenceCalendarCommand command)
        {
            MethodResult<ClassLiveWorkFlowPlanModel> queryResult = await _mediator.Send(command).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }
    }
}
