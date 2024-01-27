// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Cms.PlanetDefender.Api.Controllers
{
    using System.Net;
    using Asp.Versioning;
    using Fsel.Cms.PlanetDefender.Application.Commands.SpaceShipCmds;
    using Fsel.Cms.PlanetDefender.Application.Queries.SpaceShipQuery;
    using Fsel.Cms.PlanetDefender.Domain.Models.EntityModels;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Constants;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Shared.Constants;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;

    [ApiVersion(ApiSettings.APIVersion1)]
    [ApiVersion(ApiSettings.APIVersion1i1)]
    [Route(Settings.APIDefaultRoute + "/space-ship")]
    [ApiController]
    public class SpaceShipController : ControllerBase
    {
        private readonly IMediator _mediator;

        public SpaceShipController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Search space ship
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(MethodResult<PagingItemsModel<SpaceShipModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Search([FromQuery] SearchSpaceShipQuery query)
        {
            MethodResult<PagingItemsModel<SpaceShipModel>> queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// get list space ship according to user
        /// </summary>
        [HttpGet("get-according-to-user")]
        [ProducesResponseType(typeof(MethodResult<IList<SpaceShipModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetAccordingToUser()
        {
            var queryResult = await _mediator.Send(new GetListSpaceShipAccordingToUserQuery()).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// buy space ship
        /// </summary>
        [HttpPost("buy-space-ship")]
        [ProducesResponseType(typeof(MethodResult<bool>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> BuySpaceShip([FromBody] BuySpaceShipCommand command)
        {
            var queryResult = await _mediator.Send(command).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }
    }
}
