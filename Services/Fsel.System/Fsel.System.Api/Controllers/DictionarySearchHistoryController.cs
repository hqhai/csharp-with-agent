// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Api.Controllers
{
    using Asp.Versioning;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Constants;
    using Fsel.Core.Base;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Shared.Constants;
    using Fsel.System.Application.Queries.DictionarySearchHistoryQuery;
    using global::System.Net;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;

    [ApiVersion(ApiSettings.APIVersion1)]
    [Route(Settings.APIDefaultRoute + "/dictionary-ai/history")]
    [ApiController]
    public class DictionarySearchHistoryController : BaseController
    {
        private readonly IMediator _mediator;

        public DictionarySearchHistoryController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Get search history with paging
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(MethodResult<PagingItemsModel<Domain.Models.EntityModels.DictionarySearchHistoryModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetHistory([FromQuery] SearchDictionaryHistoryQuery query, CancellationToken cancellationToken)
        {
            MethodResult<PagingItemsModel<Domain.Models.EntityModels.DictionarySearchHistoryModel>> result =
                await _mediator.Send(query, cancellationToken);
            return result.GetActionResult();
        }

        /// <summary>
        /// Get dictionary entry from search history by ID
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(MethodResult<Fsel.Shared.Models.ShareModels.SemanticDictionaryResultModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> GetById([FromRoute] Guid id, CancellationToken cancellationToken)
        {
            MethodResult<Fsel.Shared.Models.ShareModels.SemanticDictionaryResultModel> result =
                await _mediator.Send(new GetDictionaryHistoryByIdQuery { Id = id }, cancellationToken);
            return result.GetActionResult();
        }
    }
}
