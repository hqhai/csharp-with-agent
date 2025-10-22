// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lcms.Api.Controllers
{
    using System.Net;
    using Asp.Versioning;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Constants;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Course.Application.Commands.AiPromptManagerCmd;
    using Fsel.Course.Application.Queries.AiPromptConfigQuery;
    using Fsel.Course.Application.Queries.AiPromptManagerQuery;
    using Fsel.Course.Domain.Models.CommandModels.AiPromptManager;
    using Fsel.Course.Domain.Models.EntityModels.AiPromptManagerModels;
    using Fsel.Course.Domain.Models.QueryModels.AiModelFeature;
    using Fsel.Shared.Constants;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;

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
            MethodResult<PagingItemsModel<AiManagerSearchModel>> queryResult = await _mediator.Send(query).ConfigureAwait(false);
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
            MethodResult<AiPromptManagerModel> queryResult = await _mediator.Send(new GetAiPromptManagerByIdQuery { Id = id }).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Create a Ai Model Manager
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(MethodResult<AiPromptManagerModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Create([FromBody] CreateAiPromptManagerCommand command)
        {
            MethodResult<AiPromptManagerModel> commandResult = await _mediator.Send(command).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Update a Ai Model Manager
        /// </summary>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(MethodResult<AiPromptManagerModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] UpdateAiPromptManagerCommand command)
        {
            ArgumentNullException.ThrowIfNull(command);
            command.Id = id;
            MethodResult<AiPromptManagerModel> commandResult = await _mediator.Send(command).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Delete a Ai Model Manager
        /// </summary>
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(MethodResult<AiPromptManagerModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Delete([FromRoute] Guid id)
        {
            MethodResult<bool> commandResult = await _mediator.Send(new DeleteAiPromptManagerCommand { Id = id }).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Get Ai Prompt child
        /// </summary>
        [HttpGet("child/{id}")]
        [ProducesResponseType(typeof(MethodResult<IList<AiPromptManagerModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetPromptChild([FromRoute] Guid id)
        {
            MethodResult<IList<AiPromptManagerModel>> queryResult = await _mediator.Send(new GetAiPromptChidManagerQuery { Id = id }).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Get Ai Criteria by id 
        /// </summary>
        [HttpGet("criteria/{id}")]
        [ProducesResponseType(typeof(MethodResult<IList<AICriteriaConfigsModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetCriteriaChild([FromRoute] Guid id)
        {
            MethodResult<IList<AICriteriaConfigsModel>> queryResult = await _mediator.Send(new GetAiCriteriaByIdAipromptQuery { Id = id }).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Get featue
        /// </summary>
        /// <param name="ct"></param>
        /// <returns></returns>
        [HttpGet("feature")]
        [ProducesResponseType(typeof(MethodResult<GetFeatureAiModelQuery>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<ActionResult<IReadOnlyList<GetFeatureAiModelQuery>>> Get(CancellationToken ct)
            => Ok(await _mediator.Send(new GetFeatureQuery(), ct));
    }
}
