// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Api.Controllers
{
    using Asp.Versioning;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Constants;
    using Fsel.Core.Base;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Shared.Constants;
    using Fsel.System.Application.Queries.LocationQuery;
    using Fsel.System.Domain.IRepositories;
    using Fsel.System.Domain.Models.EntityModels;
    using global::System.Net;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;

    [ApiVersion(ApiSettings.APIVersion1)]
    [ApiVersion(ApiSettings.APIVersion1i1)]
    [Route(Settings.APIDefaultRoute + "/location")]
    [ApiController]
    public class LocationController : BaseController
    {
        private readonly IMediator _mediator;
        private readonly ILocationRepository _locationRepository;

        public LocationController(IMediator mediator, ILocationRepository locationRepository)
        {
            _mediator = mediator;
            _locationRepository = locationRepository;
        }

        /// <summary>
        /// Get locations
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(MethodResult<PagingItemsModel<LocationModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Get([FromQuery] SearchLocationsQuery query)
        {
            var queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Get locations by ids
        /// </summary>
        [HttpGet("detail")]
        [ProducesResponseType(typeof(MethodResult<IList<LocationModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetDetailLocation([FromQuery] GetDetailLocationQuery query)
        {
            var queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Get locations by ids
        /// </summary>
        [HttpGet("get-by-ids")]
        [ProducesResponseType(typeof(MethodResult<IList<LocationModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetByIds([FromQuery] GetLocationsByIdsQuery query)
        {
            var queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Get locations by localId
        /// </summary>
        [HttpGet("{localId}")]
        [ProducesResponseType(typeof(MethodResult<LocationModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetLocationByLocalId([FromRoute] string localId)
        {
            var queryResult = await _mediator.Send(new GetLocationByLocalIdQuery { LocalId = localId }).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Get by global id
        /// </summary>
        [HttpGet("get-by-global-id/{id}")]
        [ProducesResponseType(typeof(MethodResult<LocationModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetLocationByGlobalId([FromRoute] string id)
        {
            var queryResult = await _mediator.Send(new GetLocationByGlobalIdQuery { GlobalId = id }).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }
    }
}
