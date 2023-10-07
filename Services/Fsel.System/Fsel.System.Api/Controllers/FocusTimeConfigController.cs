// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Api.Controllers
{
    using Fsel.Common.ActionResults;
    using Fsel.Common.Constants;
    using Fsel.System.Application.Queries.FocusTimeConfigQuery;
    using Fsel.System.Domain.Entities;
    using global::System.Collections.Generic;
    using global::System.Net;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;

    [ApiVersion(Settings.APIVersion)]
    [Route(Settings.APIDefaultRoute + "/focus-time-config")]
    [ApiController]
    public class FocusTimeConfigController : ControllerBase
    {
        private readonly IMediator _mediator;

        public FocusTimeConfigController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Get Focus Time Config Detail
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(MethodResult<IList<FocusTimeConfig>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetList()
        {
            MethodResult <IList<FocusTimeConfig>> queryResult = await _mediator.Send(new GetListFocusTimeConfigQuery()).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }
    }
}
