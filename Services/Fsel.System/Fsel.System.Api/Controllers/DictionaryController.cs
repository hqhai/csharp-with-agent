// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Api.Controllers
{
    using Asp.Versioning;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Constants;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Models.ShareModels;
    using Fsel.System.Application.Commands.DictionaryCmd;
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

        /// <summary>
        /// get dictionary new version
        /// </summary>
        [ApiVersion(ApiSettings.APIVersion1i2)]
        [HttpGet]
        [ProducesResponseType(typeof(MethodResult<IList<DictionaryQueueItemModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> SearchEngVnDictionary([FromQuery] GetDictionaryByWordVer2Query query)
        {
            var queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Import dictionary
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(MethodResult<bool>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Import([FromQuery] ImportDictionaryCommand command)
        {
            var queryResult = await _mediator.Send(command).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

    }
}
