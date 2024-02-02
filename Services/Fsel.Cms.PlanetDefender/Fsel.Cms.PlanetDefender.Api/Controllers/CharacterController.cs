// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Cms.PlanetDefender.Api.Controllers
{
    using System.Net;
    using Asp.Versioning;
    using Fsel.Cms.PlanetDefender.Application.Commands.CharacterCmds;
    using Fsel.Cms.PlanetDefender.Application.Queries.CharacterQuery;
    using Fsel.Cms.PlanetDefender.Domain.Models.EntityModels;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Constants;
    using Fsel.Shared.Constants;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;

    [ApiVersion(ApiSettings.APIVersion1)]
    [ApiVersion(ApiSettings.APIVersion1i1)]
    [Route(Settings.APIDefaultRoute + "/character")]
    [ApiController]
    public class CharacterController : ControllerBase
    {
        private readonly IMediator _mediator;

        public CharacterController(IMediator mediator)
        {
            _mediator = mediator;
        }


        /// <summary>
        /// get list character
        /// </summary>
        [HttpGet("get-list-character")]
        [ProducesResponseType(typeof(MethodResult<IList<SpaceShipModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetListCharacter([FromQuery] GetListCharacterQuery query)
        {
            var queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// get list character according to user
        /// </summary>
        [HttpGet("get-according-to-user")]
        [ProducesResponseType(typeof(MethodResult<IList<SpaceShipModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetAccordingToUser()
        {
            var queryResult = await _mediator.Send(new GetListCharacterAccordingToUserQuery()).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// buy character
        /// </summary>
        [HttpPost("buy-character")]
        [ProducesResponseType(typeof(MethodResult<bool>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> BuyCharacter([FromBody] BuyCharacterCommand command)
        {
            var queryResult = await _mediator.Send(command).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }
    }
}
