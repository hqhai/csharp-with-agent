// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Api.Controllers
{
    using Asp.Versioning;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Constants;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Shared.Constants;
    using Fsel.System.Application.Commands.OtherCmd;
    using Fsel.System.Application.Queries.TokenHistoryQuery;
    using Fsel.System.Domain.Models.EntityModels;
    using global::System.Net;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;

    [ApiVersion(ApiSettings.APIVersion1)]
    [ApiVersion(ApiSettings.APIVersion1i1)]
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
        [ProducesResponseType(typeof(MethodResult<PagingItemsModel<TokenHistoryListModel>>), (int)HttpStatusCode.OK)]
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
        [ProducesResponseType(typeof(MethodResult<CurrentUserTokenHistoryModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetUserTokenHistory([FromQuery] GetCurrentUserTokenHistoryQuery query)
        {
            var commandResult = await _mediator.Send(query).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Import User Token History
        /// </summary>
        [HttpPost("import-token-history")]
        [ProducesResponseType(typeof(MethodResult<Stream>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> ImportTokenHistory([FromQuery] ToolAddCoinToStudentsCommad command)
        {
            var commandResult = await _mediator.Send(command).ConfigureAwait(false);
            if (!commandResult.IsOK || commandResult.Result == null)
            {
                return commandResult.GetActionResult();
            }
            return File(commandResult.Result, Settings.Excels.ContentType, "tokenHistory.xlsx");
        }
    }
}
