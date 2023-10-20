// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Cms.PlanetDefender.Api.Controllers
{
    using System.Net;
    using Fsel.Cms.PlanetDefender.Application.Commands.ZMatterCmd;
    using Fsel.Cms.PlanetDefender.Application.Queries.ZMatterQuery;
    using Fsel.Cms.PlanetDefender.Domain.Models.EntityModels;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Constants;
    using Fsel.Core.Base.BaseModels;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;

    [ApiVersion(Settings.APIVersion)]
    [Route(Settings.APIDefaultRoute + "/z-matter")]
    [ApiController]
    public class ZMatterController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ZMatterController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Search Z Matter
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(MethodResult<PagingItemsModel<ZMatterModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Search([FromQuery] SearchZMatterQuery query)
        {
            MethodResult<PagingItemsModel<ZMatterModel>> queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Update status Z Matter
        /// </summary>
        [HttpPut("update-status/{id}")]
        [ProducesResponseType(typeof(MethodResult<ZMatterModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> UpdateStatus([FromRoute] Guid id, [FromBody] UpdateStatusZMatterCommand command)
        {
            ArgumentNullException.ThrowIfNull(command);
            command.Id = id;
            MethodResult<ZMatterModel> commandResult = await _mediator.Send(command).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }
    }
}
