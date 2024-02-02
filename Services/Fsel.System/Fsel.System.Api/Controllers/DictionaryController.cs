// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Api.Controllers
{
    using Asp.Versioning;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Constants;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Shared.Constants;
    using Fsel.System.Application.Queries.CourseTimeConfigQuery;
    using Fsel.System.Application.Queries.DictionaryQuery;
    using Fsel.System.Application.Services.DictionaryServices.Models;
    using global::System.Net;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;

    [ApiVersion(ApiSettings.APIVersion1)]
    [ApiVersion(ApiSettings.APIVersion1i1)]
    [Route(Settings.APIDefaultRoute + "/dictionary")]
    [ApiController]
    public class DictionaryController : ControllerBase
    {
        private readonly IMediator _mediator;

        public DictionaryController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// get dictionary
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(MethodResult<PagingItemsModel<DictionaryModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Search([FromQuery] GetDictionaryByWordQuery query)
        {
            var queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

    }
}
