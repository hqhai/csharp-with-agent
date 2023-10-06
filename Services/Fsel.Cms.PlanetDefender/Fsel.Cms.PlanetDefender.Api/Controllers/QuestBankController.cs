// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Cms.PlanetDefender.Api.Controllers
{
    using Fsel.Common.ActionResults;
    using System.Net;
    using Microsoft.AspNetCore.Mvc;
    using Fsel.Cms.PlanetDefender.Domain.Models.EntityModels;
    using MediatR;
    using Fsel.Common.Constants;
    using Fsel.Cms.PlanetDefender.Application.Commands.QuestBankCmd;
    using Fsel.Cms.PlanetDefender.Application.Services.SystemServices.Models;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Cms.PlanetDefender.Application.Queries.QuestBankQuery;

    [ApiVersion(Settings.APIVersion)]
    [Route(Settings.APIDefaultRoute + "/quest-bank")]
    [ApiController]
    public class QuestBankController : ControllerBase
    {
        private readonly IMediator _mediator;

        public QuestBankController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Mass upload game vocabulary
        /// </summary>
        [HttpPost("mass-upload")]
        [ProducesResponseType(typeof(MethodResult<IList<QuestBankModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> MassUpload([FromBody] MassUploadQuestBankCommand command)
        {
            var commandResult = await _mediator.Send(command).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Delete game vocabularies
        /// </summary>
        [HttpDelete]
        [ProducesResponseType(typeof(MethodResult<bool>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Delete([FromBody] DeleteQuestBanksCommand command)
        {
            var commandResult = await _mediator.Send(command).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Search game vocabulary
        /// </summary>
        [HttpGet("all")]
        [ProducesResponseType(typeof(MethodResult<PagingItemsModel<GameVocabularyModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> SearchAll([FromQuery] SearchAllQuestBankQuery query)
        {
            var queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Search game vocabulary
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(MethodResult<PagingItemsModel<GameVocabularyModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Search([FromQuery] SearchQuestBankQuery query)
        {
            var queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }
    }
}
