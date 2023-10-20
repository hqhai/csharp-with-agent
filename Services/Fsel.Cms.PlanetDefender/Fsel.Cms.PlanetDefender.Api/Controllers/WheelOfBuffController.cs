// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Cms.PlanetDefender.Api.Controllers
{
    using System.Net;
    using Fsel.Cms.PlanetDefender.Application.Commands.WheelOfBuffCmd;
    using Fsel.Cms.PlanetDefender.Application.Queries.WheelOfBuff;
    using Fsel.Cms.PlanetDefender.Domain.Entities;
    using Fsel.Cms.PlanetDefender.Domain.Models.EntityModels;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Constants;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;

    [ApiVersion(Settings.APIVersion)]
    [Route(Settings.APIDefaultRoute + "/wheel-of-buff")]
    [ApiController]
    public class WheelOfBuffController : ControllerBase
    {
        private readonly IMediator _mediator;

        public WheelOfBuffController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Get All wheel of buff(10)
        /// </summary>
        [HttpGet("get-list")]
        [ProducesResponseType(typeof(MethodResult<IList<WheelOfBuffModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetListWheelOfBuff()
        {
            MethodResult<IList<WheelOfBuffModel>> commandResult = await _mediator.Send(new GetWheelOfBuffQuery()).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }


        /// <summary>
        /// Update wheel of buff
        /// </summary>
        [HttpPut("update")]
        [ProducesResponseType(typeof(MethodResult<IList<WheelOfBuff>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Update([FromBody] UpdateWheelOfBuffCommand command)
        {
            ArgumentNullException.ThrowIfNull(command);
            MethodResult<IList<WheelOfBuff>> commandResult = await _mediator.Send(command).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }
    }
}
