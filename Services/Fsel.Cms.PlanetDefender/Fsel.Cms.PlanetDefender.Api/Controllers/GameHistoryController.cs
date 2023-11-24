// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Cms.PlanetDefender.Api.Controllers
{
    using System.Net;
    using Fsel.Cms.PlanetDefender.Application.Commands.GameHistoryCmd;
    using Fsel.Cms.PlanetDefender.Application.Queries.GameHistoryQuery;
    using Fsel.Cms.PlanetDefender.Domain.Models.EntityModels;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Constants;
    using Fsel.Core.Base.BaseModels;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;

    [ApiVersion(Settings.APIVersion)]
    [Route(Settings.APIDefaultRoute + "/game-history")]
    [ApiController]
    public class GameHistoryController : ControllerBase
    {
        private readonly IMediator _mediator;

        public GameHistoryController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Save Game History
        /// </summary>
        [HttpPost("save-game-history")]
        [ProducesResponseType(typeof(MethodResult<GameHistoryModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Create([FromBody] SaveGameHistoryCommand command)
        {
            MethodResult<GameHistoryModel> queryResult = await _mediator.Send(command).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Search game history
        /// </summary>
        [HttpGet("search-game-history")]
        [ProducesResponseType(typeof(MethodResult<PagingItemsModel<GameHistoryModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Search([FromQuery] SearchGameHistoryQuery query)
        {
            MethodResult<PagingItemsModel<GameHistoryModel>> queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// get report
        /// </summary>
        [HttpGet("get-game-history/{id}")]
        [ProducesResponseType(typeof(MethodResult<GameHistoryModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetReport([FromRoute] Guid id)
        {
            var commandResult = await _mediator.Send(new GetGameHistoryQuery { Id = id }).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }
    }
}
