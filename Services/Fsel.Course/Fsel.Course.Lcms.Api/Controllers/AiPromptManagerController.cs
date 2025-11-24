// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lcms.Api.Controllers
{
    using System.Net;
    using Application.Commands.AiPromptManagerCmd;
    using Application.Queries.AiPromptManagerQuery;
    using Asp.Versioning;
    using Common.ActionResults;
    using Common.Constants;
    using Core.Base.BaseModels;
    using Domain.Models.CommandModels.AiPromptManager;
    using Domain.Models.EntityModels.AiPromptManagerModels;
    using Domain.Models.QueryModels.AiPromptConfig;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;
    using Shared.Constants;

    [ApiController]
    [ApiVersion(ApiSettings.APIVersion1)]
    [ApiVersion(ApiSettings.APIVersion1i1)]
    [Route(Settings.APIDefaultRoute + "/ai-prompt-manager")]
    //[Common.Attributes.Permission(role: nameof(EnumRole.MasterAdmin))]
    public class AiPromptManagerController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AiPromptManagerController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Search Ai Model Manager
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(MethodResult<PagingItemsModel<AiManagerSearchModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Search([FromQuery] GetAiPromptManagerQuery query)
        {
            var queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Get Ai Model Manager
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(MethodResult<AiPromptManagerModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Get([FromRoute] Guid id)
        {
            var queryResult = await _mediator.Send(new GetAiPromptManagerByIdQuery { Id = id }).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Create an Ai Model Manager
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(MethodResult<AiPromptManagerModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Create([FromBody] CreateAiPromptManagerCommand command)
        {
            var commandResult = await _mediator.Send(command).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Update an Ai Model Manager
        /// </summary>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(MethodResult<AiPromptManagerModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] UpdateAiPromptManagerCommand command)
        {
            ArgumentNullException.ThrowIfNull(command);
            command.Id = id;
            var commandResult = await _mediator.Send(command).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Delete an Ai Model Manager
        /// </summary>
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(MethodResult<AiPromptManagerModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Delete([FromRoute] Guid id)
        {
            var commandResult = await _mediator.Send(new DeleteAiPromptManagerCommand { Id = id }).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Get feature
        /// </summary>
        /// <param name="ct"></param>
        /// <returns></returns>
        [HttpGet("feature")]
        [ProducesResponseType(typeof(MethodResult<GetFeatureAiModelQuery>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<ActionResult<IReadOnlyList<GetFeatureAiModelQuery>>> Get(CancellationToken ct)
        {
            return Ok(await _mediator.Send(new GetFeatureQuery(), ct));
        }
    }
}
