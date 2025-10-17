// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Api.Controllers
{
    using System.Net;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Constants;
    using Fsel.Ordering.Application.Commands.Campus;
    using Fsel.Shared.Attributes;
    using Fsel.Shared.Constants;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;

    [ApiVersions(ApiSettings.APIVersion1)]
    [Route(Settings.APIDefaultRoute + "/campus")]
    [ApiController]
    public class CampusController : ControllerBase
    {
        private readonly IMediator _mediator;

        public CampusController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Create order for students campus
        /// </summary>
        [HttpPost("create-order-for-students-campus")]
        [ProducesResponseType(typeof(MethodResult<bool>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> CreateOrders([FromBody] CreateOrdersForStudentCampusCommand command)
        {
            var queryResult = await _mediator.Send(command).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Delete order of students campus
        /// </summary>
        [HttpPost("delete-order-of-students-campus")]
        [ProducesResponseType(typeof(MethodResult<bool>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> DeleteOrders([FromBody] DeleteOrderOfStudentsCampusCommand command)
        {
            var queryResult = await _mediator.Send(command).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }
    }
}
