// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Api.Controllers
{
    using Asp.Versioning;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Attributes;
    using Fsel.Common.Constants;
    using Fsel.Core.Base;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Shared.Constants;
    using Fsel.System.Application.Queries.SchoolQuery;
    using Fsel.System.Domain.IRepositories;
    using Fsel.System.Domain.Models.EntityModels;
    using global::System.Net;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;

    [ApiVersion(ApiSettings.APIVersion1)]
    [ApiVersion(ApiSettings.APIVersion1i1)]
    [Route(Settings.APIDefaultRoute + "/school")]
    [ApiController]
    public class SchoolController : BaseController
    {
        private readonly IMediator _mediator;
        private readonly ISchoolRepository _schoolRepository;

        public SchoolController(IMediator mediator, ISchoolRepository schoolRepository)
        {
            _mediator = mediator;
            _schoolRepository = schoolRepository;
        }

        /// <summary>
        /// Execute-list-query
        /// </summary>
        [HttpPost("execute-list-query")]
        [ProducesResponseType(typeof(MethodResult<IList<SchoolModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [Permission]
        public async Task<IActionResult> ExecuteList([FromBody] BaseQueryModel cmd)
        {
            SetQuery(cmd);
            var result = await _schoolRepository.GetListResultAsync<SchoolModel>(cmd);
            return result.GetActionResult();
        }

        /// <summary>
        /// Search School
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(MethodResult<PagingItemsModel<SchoolModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Search([FromQuery] SearchSchoolQuery query)
        {
            MethodResult<PagingItemsModel<SchoolModel>> queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Get List School
        /// </summary>
        [HttpPost("get-by-ids")]
        [ProducesResponseType(typeof(MethodResult<IList<SchoolModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Gets([FromBody] IList<Guid>? ids)
        {
            MethodResult<IList<SchoolModel>> queryResult = await _mediator.Send(new GetSchoolsByIdsQuery { Ids = ids }).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }
    }
}
