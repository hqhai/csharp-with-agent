// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Training.Api.Controllers.Teacher
{
    using Fsel.Common.ActionResults;
    using System.Net;
    using Fsel.Common.Constants;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Shared.Enums;
    using Fsel.Training.Application.Queries.ChangeLiveSessionQuery;
    using Fsel.Training.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;

    [ApiVersion(Settings.APIVersion)]
    [Route(Settings.APIDefaultRoute + "/teacher/cancel-schedule")]
    [ApiController]
    [Authorize(Roles = nameof(EnumRole.Teacher))]
    public class ChangeLiveSessionController
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
        [ProducesResponseType(typeof(MethodResult<PagingItemsModel<CancelScheduleLiveModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Search([FromQuery] SearchChangeLiveSessionTeacherQuery query)
        {
            MethodResult<PagingItemsModel<CancelScheduleLiveModel>> queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }
    }
}
