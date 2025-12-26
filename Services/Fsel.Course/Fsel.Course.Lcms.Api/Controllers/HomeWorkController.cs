// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lcms.Api.Controllers
{
    using System.Net;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Constants;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Course.Application.Commands.HomeWorkCmd;
    using Fsel.Course.Application.Queries.HomeWorkQuery;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;
    using Asp.Versioning;
    using Fsel.Shared.Constants;

    [ApiVersion(ApiSettings.APIVersion1)]
    [ApiVersion(ApiSettings.APIVersion1i1)]
    [Route(Settings.APIDefaultRoute + "/home-work")]
    [ApiController]
    [Common.Attributes.Permission(role: nameof(EnumRole.MasterAdmin))]
    public class HomeWorkController : ControllerBase
    {
        private readonly IMediator _mediator;

        public HomeWorkController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Search Home Work
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(MethodResult<PagingItemsModel<HomeWorkSearchModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Search([FromQuery] SearchHomeWorkQuery query)
        {
            MethodResult<PagingItemsModel<HomeWorkSearchModel>> queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Get Home Work
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(MethodResult<HomeWorkModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Get([FromRoute] Guid id)
        {
            MethodResult<HomeWorkModel> queryResult = await _mediator.Send(new GetHomeWorkQuery { Id = id }).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Create a Home Work
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(MethodResult<HomeWorkModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Create([FromBody] CreateHomeWorkCommand command)
        {
            MethodResult<HomeWorkModel> commandResult = await _mediator.Send(command).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Delete a Home Work
        /// </summary>
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(MethodResult<HomeWorkModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Delete([FromRoute] Guid id)
        {
            MethodResult<bool> commandResult = await _mediator.Send(new DeleteHomeWorkCommand { Id = id }).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Update a Home Work
        /// </summary>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(MethodResult<HomeWorkModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] UpdateHomeWorkCommand command)
        {
            ArgumentNullException.ThrowIfNull(command);
            command.Id = id;
            MethodResult<HomeWorkModel> commandResult = await _mediator.Send(command).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Get Home Work By OriginalId
        /// </summary>
        [HttpGet("original/{originalId}")]
        [ProducesResponseType(typeof(MethodResult<HomeWorkModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetHomeWorkByOriginal([FromRoute] Guid originalId)
        {
            MethodResult<HomeWorkModel> queryResult = await _mediator.Send(new GetHomeWorkByOriginalIdQuery { OriginalId = originalId }).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Get detail Home Work By OriginalId
        /// </summary>
        [HttpGet("detail/{originalId}")]
        [ProducesResponseType(typeof(MethodResult<HomeWorkModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetDetail([FromRoute] Guid originalId)
        {
            MethodResult<HomeWorkModel> queryResult = await _mediator.Send(new GetHomeWorkDetailByOriginalIdQuery { OriginalId = originalId }).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }
    }
}
