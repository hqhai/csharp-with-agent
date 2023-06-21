// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Api.Controllers.Admin
{
    using System.Net;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Constants;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Fsel.Course.Lms.Application.Queries.PlacementTestQuery;

    [ApiVersion(Settings.APIVersion)]
    [Route(Settings.APIDefaultRoute + "/placement-test/admin")]
    [ApiController]
    [Authorize(Roles = nameof(EnumRole.Admin))]
    public class PlacementTestController : ControllerBase
    {
        private readonly IMediator _mediator;

        public PlacementTestController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// get average pt point
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(MethodResult<List<AveragePTPointModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Get([FromBody] GetAveragePTPointByIdsQuery query)
        {
            MethodResult<List<AveragePTPointModel>> queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }
    }
}
