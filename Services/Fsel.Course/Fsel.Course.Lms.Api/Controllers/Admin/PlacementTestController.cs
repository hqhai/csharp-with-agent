// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Api.Controllers.Admin
{
    using System.Net;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Constants;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Lms.Application.Queries.PlacementTestQuery;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;

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

        /// <summary>
        /// get average pt point
        /// </summary>
        [HttpPost("get-pt-point-by-ids")]
        [ProducesResponseType(typeof(MethodResult<List<StudentPTPointModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetPTPointByIds([FromBody] IList<Guid>? ids)
        {
            MethodResult<List<StudentPTPointModel>> queryResult = await _mediator.Send(new GetPTPointByIdsQuery { StudentIds = ids }).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }
    }
}
