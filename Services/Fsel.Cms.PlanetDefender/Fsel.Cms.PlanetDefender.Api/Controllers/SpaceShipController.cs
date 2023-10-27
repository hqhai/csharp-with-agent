// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Cms.PlanetDefender.Api.Controllers
{
    using Fsel.Cms.PlanetDefender.Application.Queries.WheelOfBuff;
    using Fsel.Cms.PlanetDefender.Domain.Models.EntityModels;
    using Fsel.Common.ActionResults;
    using System.Net;
    using Fsel.Common.Constants;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;
    using Fsel.Cms.PlanetDefender.Application.Queries.SpaceShipQuery;
    using Fsel.Cms.PlanetDefender.Application.Queries.ZMatterQuery;
    using Fsel.Core.Base.BaseModels;

    [ApiVersion(Settings.APIVersion)]
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
    }
}
