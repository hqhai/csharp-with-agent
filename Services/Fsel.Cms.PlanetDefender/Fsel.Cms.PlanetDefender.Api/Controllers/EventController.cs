// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Cms.PlanetDefender.Api.Controllers
{
    using System.Net;
    using Asp.Versioning;
    using Fsel.Cms.PlanetDefender.Application.Commands.NewsAndUpdateCmd;
    using Fsel.Cms.PlanetDefender.Application.Queries.NewsAndUpdateQuery;
    using Fsel.Cms.PlanetDefender.Domain.Models.EntityModels;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Constants;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Shared.Constants;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;

    [ApiVersion(ApiSettings.APIVersion1)]
    [ApiVersion(ApiSettings.APIVersion1i1)]
    [Route(Settings.APIDefaultRoute + "/news-and-update")]
    [ApiController]
    public class EventController : ControllerBase
    {
        private readonly IMediator _mediator;

        public EventController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// get list album
        /// </summary>
        [HttpGet("get-list-event")]
        [ProducesResponseType(typeof(MethodResult<IList<EventModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetListAlbum()
        {
            var commandResult = await _mediator.Send(new GetListEventQuery()).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Search album
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(MethodResult<PagingItemsModel<EventModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Search([FromQuery] SearchEventQuery query)
        {
            MethodResult<PagingItemsModel<EventModel>> queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Create a album
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(MethodResult<EventModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Create([FromBody] CreateEventCommand command)
        {
            MethodResult<EventModel> queryResult = await _mediator.Send(command).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Update a album
        /// </summary>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(MethodResult<EventModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] UpdateEventCommand command)
        {
            ArgumentNullException.ThrowIfNull(command);
            command.Id = id;
            MethodResult<EventModel> commandResult = await _mediator.Send(command).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Delete a album
        /// </summary>
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(MethodResult<EventModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Delete([FromRoute] Guid id)
        {
            MethodResult<bool> commandResult = await _mediator.Send(new DeleteEventCommand { Id = id }).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Get album
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(MethodResult<EventModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Get([FromRoute] Guid id)
        {
            MethodResult<EventModel> commandResult = await _mediator.Send(new GetEventQuery { Id = id }).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }
    }
}
