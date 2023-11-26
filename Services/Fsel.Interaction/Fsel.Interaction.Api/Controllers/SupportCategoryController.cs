// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Api.Controllers
{
    using System.Net;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Constants;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Interaction.Application.Commands.SupportCategoryCmd;
    using Fsel.Interaction.Application.Queries.SupportCategoryQuery;
    using Fsel.Interaction.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;
    using Asp.Versioning;
    using Fsel.Shared.Constants;

    [ApiVersion(ApiSettings.APIVersion1)][ApiVersion(ApiSettings.APIVersion1i1)]
    [Route(Settings.APIDefaultRoute + "/support-category")]
    [ApiController]
    public class SupportCategoryController : ControllerBase
    {
        private readonly IMediator _mediator;

        public SupportCategoryController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Search Support Category
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(MethodResult<PagingItemsModel<SupportCategoryModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Search([FromQuery] SearchSupportCategoryQuery query)
        {
            MethodResult<PagingItemsModel<SupportCategoryModel>> queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Create a Support Category
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(MethodResult<SupportCategoryModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Create([FromBody] CreateSupportCategoryCommand command)
        {
            MethodResult<SupportCategoryModel> queryResult = await _mediator.Send(command).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Update a Support Category
        /// </summary>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(MethodResult<SupportCategoryModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] UpdateSupportCategoryCommand command)
        {
            ArgumentNullException.ThrowIfNull(command);
            command.Id = id;
            MethodResult<SupportCategoryModel> commandResult = await _mediator.Send(command).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Delete a Support Category
        /// </summary>
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(MethodResult<SupportCategoryModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Delete([FromRoute] Guid id)
        {
            MethodResult<bool> commandResult = await _mediator.Send(new DeleteSupportCategoryCommand { Id = id }).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Get Support Category
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(MethodResult<SupportCategoryModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Get([FromRoute] Guid id)
        {
            MethodResult<SupportCategoryModel> commandResult = await _mediator.Send(new GetSupportCategoryQuery { Id = id }).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Update a Support Category
        /// </summary>
        [HttpPut("update-status/{id}")]
        [ProducesResponseType(typeof(MethodResult<SupportCategoryModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> UpdateStatus([FromRoute] Guid id, [FromBody] UpdateStatusSupportCategoryCommand command)
        {
            ArgumentNullException.ThrowIfNull(command);
            command.Id = id;
            MethodResult<SupportCategoryModel> commandResult = await _mediator.Send(command).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Get Support Category
        /// </summary>
        [HttpGet("list-support-category")]
        [ProducesResponseType(typeof(MethodResult<IList<SupportCategoryModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Get()
        {
            MethodResult<IList<SupportCategoryModel>> commandResult = await _mediator.Send(new GetListSupportCategoryQuery()).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }
    }
}
