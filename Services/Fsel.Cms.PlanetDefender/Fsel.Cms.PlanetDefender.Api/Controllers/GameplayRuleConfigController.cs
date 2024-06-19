// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Cms.PlanetDefender.Api.Controllers
{
    using System.Net;
    using Asp.Versioning;
    using Fsel.Shared.Constants;
    using Fsel.Cms.PlanetDefender.Application.Commands.GameplayRuleConfigCmd;
    using Fsel.Cms.PlanetDefender.Application.Queries.GameplayRuleConfigs;
    using Fsel.Cms.PlanetDefender.Domain.Models.QueryModels.GameplayRuleConfigs;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Constants;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;

    [ApiVersion(ApiSettings.APIVersion1)]
    [ApiVersion(ApiSettings.APIVersion1i1)]
    [Route(Settings.APIDefaultRoute + "/gameplay-rule-config")]
    [ApiController]
    public class GameplayRuleConfigController : ControllerBase
    {
        private readonly IMediator _mediator;

        public GameplayRuleConfigController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// save list gameplaytimeconfig
        /// </summary>
        [HttpPost("save-list-game-play-rule-config")]
        [ProducesResponseType(typeof(MethodResult<bool>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> SaveList([FromBody] SaveListGameplayRuleConfigCommand command)
        {
            var commandResult = await _mediator.Send(command).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// get list gameplaytimeconfig
        /// </summary>
        [HttpGet("get-list-game-play-rule-config")]
        [ProducesResponseType(typeof(MethodResult<IList<GameplayRuleConfigsModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetList()
        {
            var commandResult = await _mediator.Send(new GetGameplayRuleConfigsQuery { }).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }
    }
}
