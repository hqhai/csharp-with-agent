// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Api.Controllers
{
    using Asp.Versioning;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Constants;
    using Fsel.Core.Base;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Models.ShareModels;
    using Fsel.System.Application.Commands.DictionaryAICmd;
    using Fsel.System.Application.Queries.DictionaryAIQuery;
    using global::System.Collections.Generic;
    using global::System.Net;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;

    [ApiVersion(ApiSettings.APIVersion1)]
    [Route(Settings.APIDefaultRoute + "/dictionary-ai")]
    [ApiController]
    public class DictionaryAIController : BaseController
    {
        private readonly IMediator _mediator;

        public DictionaryAIController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Search dictionary using semantic search (pgvector)
        /// </summary>
        [HttpPost("search")]
        [ProducesResponseType(typeof(MethodResult<SemanticDictionaryResultModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Search([FromBody] SearchDictionaryAICommand command, CancellationToken cancellationToken)
        {
            MethodResult<SemanticDictionaryResultModel> result = await _mediator.Send(command, cancellationToken);
            return result.GetActionResult();
        }

        /// <summary>
        /// Generate new dictionary entry (skip cache)
        /// </summary>
        [HttpPost("generate")]
        [ProducesResponseType(typeof(MethodResult<SemanticDictionaryResultModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Generate([FromBody] GenerateDictionaryAICommand command, CancellationToken cancellationToken)
        {
            MethodResult<SemanticDictionaryResultModel> result = await _mediator.Send(command, cancellationToken);
            return result.GetActionResult();
        }

        /// <summary>
        /// Get dictionary entry by ID
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(MethodResult<SemanticDictionaryResultModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> GetById([FromRoute] Guid id, CancellationToken cancellationToken)
        {
            MethodResult<SemanticDictionaryResultModel> result = await _mediator.Send(new GetDictionaryAIByIdQuery { Id = id }, cancellationToken);
            return result.GetActionResult();
        }
    }
}
