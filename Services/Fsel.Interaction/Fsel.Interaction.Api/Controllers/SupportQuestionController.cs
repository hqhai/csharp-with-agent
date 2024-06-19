// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Api.Controllers
{
    using System.Net;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Constants;
    using Fsel.Core.Base;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Interaction.Application.Commands.SupportQuetionCmd;
    using Fsel.Interaction.Application.Queries.SupportQuestionQuery;
    using Fsel.Interaction.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;
    using Asp.Versioning;
    using Fsel.Shared.Constants;

    [ApiVersion(ApiSettings.APIVersion1)][ApiVersion(ApiSettings.APIVersion1i1)]
    [Route(Settings.APIDefaultRoute + "/support-question")]
    [ApiController]
    public class SupportQuestionController : BaseController
    {
        private readonly IMediator _mediator;

        public SupportQuestionController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Search Support Question
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(MethodResult<PagingItemsModel<SupportQuestionModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Search([FromQuery] SearchSupportQuestionQuery query)
        {
            SetQuery(query);
            MethodResult<PagingItemsModel<SupportQuestionModel>> queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Create a Support Question
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(MethodResult<SupportQuestionModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Create([FromBody] CreateSupportQuetionCommand command)
        {
            MethodResult<SupportQuestionModel> queryResult = await _mediator.Send(command).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Update a Support Question
        /// </summary>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(MethodResult<SupportQuestionModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] UpdateSupportQuestionComand command)
        {
            ArgumentNullException.ThrowIfNull(command);
            command.Id = id;
            MethodResult<SupportQuestionModel> commandResult = await _mediator.Send(command).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Delete a Support Question
        /// </summary>
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(MethodResult<SupportQuestionModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Delete([FromRoute] Guid id)
        {
            MethodResult<bool> commandResult = await _mediator.Send(new DeleteSupportQuestionCommand { Id = id }).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Get Support Question
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(MethodResult<SupportQuestionModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Get([FromRoute] Guid id)
        {
            MethodResult<SupportQuestionModel> commandResult = await _mediator.Send(new GetSupportQuestionQuery { Id = id }).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Update a Support Question
        /// </summary>
        [HttpPut("update-status-active/{id}")]
        [ProducesResponseType(typeof(MethodResult<SupportQuestionModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> UpdateStatus([FromRoute] Guid id, [FromBody] UpdateStatusSupportQuestionCommand command)
        {
            ArgumentNullException.ThrowIfNull(command);
            command.Id = id;
            MethodResult<SupportQuestionModel> commandResult = await _mediator.Send(command).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Update a Support Question
        /// </summary>
        [HttpPut("update-frequent-status/{id}")]
        [ProducesResponseType(typeof(MethodResult<SupportQuestionModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> UpdateStatus([FromRoute] Guid id, [FromBody] UpdateFrequentStatusSupportQuestionCommand command)
        {
            ArgumentNullException.ThrowIfNull(command);
            command.Id = id;
            MethodResult<SupportQuestionModel> commandResult = await _mediator.Send(command).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }
    }
}
