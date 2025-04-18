// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Api.Controllers.Admin
{
    using System.Net;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Constants;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Lms.Application.Commands.PlacementTestCmd.V1i1;
    using Fsel.Course.Lms.Application.Queries.PlacementTestQuery;
    using Fsel.Shared.Attributes;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;

    [ApiVersions(ApiSettings.APIVersion1)]
    [Route(Settings.APIDefaultRoute + "/placement-test/admin")]
    [ApiController]
    [Common.Attributes.Permission(roles: new string[] { nameof(EnumRole.Admin), nameof(EnumRole.AdminSchool), nameof(EnumRole.CSO) })]
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

        /// <summary>
        /// Save PlacementTest Done
        /// </summary>
        [HttpPost("save-done")]
        [ProducesResponseType(typeof(MethodResult<bool>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> SavePlacementTestDone([FromBody] SavePlacementTestDoneCommand command)
        {
            MethodResult<bool> queryResult = await _mediator.Send(command).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }
    }
}
