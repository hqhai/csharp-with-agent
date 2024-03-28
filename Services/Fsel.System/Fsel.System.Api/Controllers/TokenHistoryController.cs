// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Api.Controllers
{
    using Asp.Versioning;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Constants;
    using Fsel.Shared.Constants;
    using Fsel.System.Application.Queries.TokenHistoryQuery;
    using Fsel.System.Domain.Models.EntityModels;
    using global::System.Net;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;

    [ApiVersion(ApiSettings.APIVersion1)]
    [Route(Settings.APIDefaultRoute + "/token-history")]
    [ApiController]
    public class TokenHistoryController : ControllerBase
    {
        private readonly IMediator _mediator;

        public TokenHistoryController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Search Token History
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(MethodResult<IList<TokenHistoryModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetList([FromQuery] SearchTokenHistoryQuery query)
        {
            var commandResult = await _mediator.Send(query).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Get User Token History
        /// </summary>
        [HttpGet("user-token-history")]
        [ProducesResponseType(typeof(MethodResult<IList<CurrentUserTokenHistoryModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetUserTokenHistory([FromQuery] GetCurrentUserTokenHistoryQuery query)
        {
            var commandResult = await _mediator.Send(query).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }
    }
}
