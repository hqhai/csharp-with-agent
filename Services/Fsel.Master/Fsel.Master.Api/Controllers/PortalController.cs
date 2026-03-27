// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Master.Api.Controllers
{
    using System.Net;
    using Asp.Versioning;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Constants;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Master.Application.Queries.PortalRankingQuery;
    using Fsel.Master.Domain.Models.EntityModels;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;

    [ApiVersion(ApiSettings.APIVersion1)]
    [ApiVersion(ApiSettings.APIVersion1i1)]
    [Route(Settings.APIDefaultRoute + "/portal")]
    [ApiController]
    public class PortalController : ControllerBase
    {
        private readonly IMediator _mediator;

        public PortalController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("registration-rate")]
        [ProducesResponseType(typeof(MethodResult<PagingItemsModel<PortalRankingModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetRankingByRegistrationRate([FromQuery] GetTopUnitsByRegistrationRateQuery query)
        {
            MethodResult<PagingItemsModel<PortalRankingModel>> queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        [HttpGet("active-students")]
        [ProducesResponseType(typeof(MethodResult<PagingItemsModel<PortalRankingModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetRankingByActiveStudents([FromQuery] GetTopUnitsByActiveStudentsQuery query)
        {
            MethodResult<PagingItemsModel<PortalRankingModel>> queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }
    }
}
