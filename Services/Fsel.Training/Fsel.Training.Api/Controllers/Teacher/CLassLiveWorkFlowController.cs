// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Training.Api.Controllers.Teacher
{
    using System.Net;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Constants;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Shared.Enums;
    using Fsel.Training.Application.Queries.ClassLiveWorkFlowQuery;
    using Fsel.Training.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;

    [ApiVersion(Settings.APIVersion)]
    [Route(Settings.APIDefaultRoute + "/teacher/class-live-work")]
    [ApiController]
    [Authorize(Roles = nameof(EnumRole.Teacher))]
    public class CLassLiveWorkFlowController : ControllerBase
    {
        private readonly IMediator _mediator;

        public CLassLiveWorkFlowController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Search Teacher Free Date
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(MethodResult<PagingItemsModel<SearchClassLiveWorkFlowModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> Search([FromQuery] SearchClassLiveWorkFlowQuery query)
        {
            MethodResult<PagingItemsModel<SearchClassLiveWorkFlowModel>> commandResult = await _mediator.Send(query).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }
    }
}
