// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Training.Api.Controllers.Teacher
{
    using Fsel.Common.ActionResults;
    using System.Net;
    using Fsel.Common.Constants;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Shared.Enums;
    using Fsel.Training.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Fsel.Training.Application.Queries.CancelScheduleLiveQuery;

    [ApiVersion(Settings.APIVersion)]
    [Route(Settings.APIDefaultRoute + "/teacher/cancel-schedule")]
    [ApiController]
    [Authorize(Roles = nameof(EnumRole.Teacher))]
    public class CancelScheduleLiveController
    {
        private readonly IMediator _mediator;

        public CancelScheduleLiveController(IMediator mediator)
        {
            _mediator = mediator;
        }
        /// <summary>
        /// Search Change Live Session
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(MethodResult<PagingItemsModel<CancelScheduleLiveModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Search([FromQuery] SearchCancelScheduleLiveTeacherQuery query)
        {
            MethodResult<PagingItemsModel<CancelScheduleLiveModel>> queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Get Change Live Session
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(MethodResult<CancelScheduleLiveInfoModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Get([FromRoute] Guid id)
        {
            MethodResult<CancelScheduleLiveInfoModel> queryResult = await _mediator.Send(new GetCancelScheduleLiveQuery { Id = id }).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }
    }
}
