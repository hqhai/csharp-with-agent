// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Api.Controllers
{
    using System.Net;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Constants;
    using Fsel.Identity.Application.Commands.UserCmd;
    using Fsel.Identity.Application.Queries.PlatformQuery;
    using Fsel.Identity.Application.Queries.StudentQuery;
    using Fsel.Identity.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;

    [ApiVersion(Settings.APIVersion)]
    [Route(Settings.APIDefaultRoute + "/platform")]
    [ApiController]
    public class PlatformController : ControllerBase
    {
        private readonly IMediator _mediator;

        public PlatformController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Get all platform
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(MethodResult<IList<PlatformModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetAll()
        {
            MethodResult<IList<PlatformModel>> commandResult = await _mediator.Send(new GetAllPlatformQuery { }).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Get platforms by type
        /// </summary>
        [HttpGet("get-platforms-by-type")]
        [ProducesResponseType(typeof(MethodResult<IList<PlatformModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetPlatformByType([FromQuery] GetPlatformsByTypeQuery query)
        {
            MethodResult<IList<PlatformModel>> commandResult = await _mediator.Send(query).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Get all platform
        /// </summary>
        [HttpGet("get-students-in-platform")]
        [ProducesResponseType(typeof(MethodResult<IList<StudentInPlatformModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetStudentsInPlatform([FromQuery] GetStudentsInPlatformQuery query)
        {
            MethodResult<IList<StudentInPlatformModel>> commandResult = await _mediator.Send(query).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// change status account
        /// </summary>
        [HttpPost("change-status-account")]
        [ProducesResponseType(typeof(MethodResult<bool>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetAll([FromBody] ChangeStatusAccountCommand command)
        {
            MethodResult<bool> commandResult = await _mediator.Send(command).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }
    }
}
