// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Api.Controllers.Admins
{
    using Fsel.Common.ActionResults;
    using Fsel.Common.Constants;
    using Fsel.Core.Base.BaseModels;
    using Fsel.System.Application.Commands.QuestBoardCmd;
    using Fsel.System.Application.Queries.QuestBoardQuery;
    using Fsel.System.Domain.Models.EntityModels;
    using global::System.Net;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;

    [ApiVersion(Settings.APIVersion)]
    [Route(Settings.APIDefaultRoute + "/admin/quest-board")]
    [ApiController]
    public class QuestBoardController : ControllerBase
    {
        private readonly IMediator _mediator;

        public QuestBoardController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Search Quest Board
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(MethodResult<PagingItemsModel<QuestBoardSearchModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Search([FromQuery] SearchQuestBoardByAdminQuery query)
        {
            var queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Get Quest Boards
        /// </summary>
        [HttpGet("list-quest-board-active")]
        [ProducesResponseType(typeof(MethodResult<IList<QuestBoardSearchModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Gets([FromQuery] GetQuestBoardsActiveByAdminQuery query)
        {
            var queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Get Quest Board
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(MethodResult<QuestBoardModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Get([FromRoute] Guid id)
        {
            MethodResult<QuestBoardModel> commandResult = await _mediator.Send(new GetQuestBoardByAdminQuery { Id = id }).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Get Quest Board By DependentId
        /// </summary>
        [HttpGet("dependent/{dependentId}")]
        [ProducesResponseType(typeof(MethodResult<IList<QuestBoardModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetQuestBoardByDependentId([FromRoute] Guid dependentId)
        {
            MethodResult<IList<QuestBoardModel>> commandResult = await _mediator.Send(new GetQuestBoardByDependentIdQuery { DependentId = dependentId }).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Active Quest Board
        /// </summary>
        [HttpPost("active")]
        [ProducesResponseType(typeof(MethodResult<QuestBoardModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Active([FromQuery] ActiveQuestBoardCommand command)
        {
            MethodResult<QuestBoardModel> commandResult = await _mediator.Send(command).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Create Quest Board
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(MethodResult<QuestBoardModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Create([FromBody] CreateQuestBoardCommand command)
        {
            MethodResult<QuestBoardModel> commandResult = await _mediator.Send(command).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Delete Quest Board
        /// </summary>
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(MethodResult<bool>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Delete([FromRoute] Guid id)
        {
            MethodResult<bool> commandResult = await _mediator.Send(new DeleteQuestBoardCommand { Id = id }).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Delete Quest Boards
        /// </summary>
        [HttpDelete]
        [ProducesResponseType(typeof(MethodResult<bool>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> DeleteByIds([FromBody] DeleteQuestBoardsCommand command)
        {
            MethodResult<bool> commandResult = await _mediator.Send(command).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }
    }
}
