// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Api.Controllers
{
    using Fsel.Common.ActionResults;
    using Fsel.Common.Constants;
    using Fsel.Core.Base.BaseModels;
    using Fsel.System.Application.Commands.GameVocabularyCmd;
    using Fsel.System.Application.Queries.GameVocabularies;
    using Fsel.System.Domain.Models.EntityModels;
    using global::System.Net;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;

    [ApiVersion(Settings.APIVersion)]
    [Route(Settings.APIDefaultRoute + "/game-vocabulary")]
    [ApiController]
    public class GameVocabularyController : ControllerBase
    {
        private readonly IMediator _mediator;

        public GameVocabularyController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Create game vocabulary
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(MethodResult<GameVocabularyModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Create([FromBody] CreateGameVocabularyCommand command)
        {
            var commandResult = await _mediator.Send(command).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Update game vocabulary
        /// </summary>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(MethodResult<GameVocabularyModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] UpdateGameVocabularyCommand command)
        {
            ArgumentNullException.ThrowIfNull(command);
            command.Id = id;
            var commandResult = await _mediator.Send(command).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Delete game vocabularies
        /// </summary>
        [HttpDelete]
        [ProducesResponseType(typeof(MethodResult<bool>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Delete([FromBody] DeleteGameVocabulariesCommand command)
        {
            var commandResult = await _mediator.Send(command).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Search game vocabulary
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(MethodResult<PagingItemsModel<GameVocabularyModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Search([FromQuery] SearchGameVocabularyQuery query)
        {
            var queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Get game vocabulary by id
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(MethodResult<GameVocabularyModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetById([FromRoute] Guid id)
        {
            var queryResult = await _mediator.Send(new GetGameVocabularyByIdQuery { Id = id }).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }
    }
}
