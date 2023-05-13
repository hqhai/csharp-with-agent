// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Api.Controllers
{
    using System.Net;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Constants;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Lms.Application.Commands.PlacementTestCmd;
    using Fsel.Course.Lms.Application.Queries.CourseQuery;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;

    [ApiVersion(Settings.APIVersion)]
    [Route(Settings.APIDefaultRoute + "/placement-test")]
    [ApiController]
    [Authorize(Roles = nameof(EnumRole.Student))]
    public class PlacmentTestController : ControllerBase
    {
        private readonly IMediator _mediator;

        public PlacmentTestController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Start PlacementTest
        /// </summary>
        [HttpGet("start-placement-test")]
        [ProducesResponseType(typeof(MethodResult<bool>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> StartPlacementTest([FromQuery] Guid placmentTestId)
        {
            MethodResult<bool> queryResult = await _mediator.Send(new StartPlacementTestCommand { PlacementTestId = placmentTestId }).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }
    }
}
