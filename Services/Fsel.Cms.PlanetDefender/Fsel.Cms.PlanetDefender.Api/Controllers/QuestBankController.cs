// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Cms.PlanetDefender.Api.Controllers
{
    using Fsel.Cms.PlanetDefender.Application.Commands;
    using Fsel.Common.ActionResults;
    using System.Net;
    using Microsoft.AspNetCore.Mvc;
    using Fsel.Cms.PlanetDefender.Domain.Models.EntityModels;
    using MediatR;
    using Fsel.Common.Constants;

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
    }
}
