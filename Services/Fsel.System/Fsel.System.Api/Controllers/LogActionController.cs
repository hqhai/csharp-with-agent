// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Api.Controllers
{
    using Fsel.Common.ActionResults;
    using Fsel.Common.Constants;
    using Fsel.System.Application.Queries.LogActionQuery;
    using Fsel.System.Domain.Models.EntityModels;
    using global::System.Net;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;

    [ApiVersion(Settings.APIVersion)]
    [Route(Settings.APIDefaultRoute + "/log-action")]
    [ApiController]
    public class LogActionController : ControllerBase
    {
        private readonly IMediator _mediator;

        public LogActionController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Get LogActions by UnitId
        /// </summary>
        [HttpGet("{userId}")]
        [ProducesResponseType(typeof(MethodResult<IList<LogActionModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetLogActionsByUserId([FromRoute] Guid userId)
        {
            MethodResult<IList<LogActionModel>> commandResult = await _mediator.Send(new GetLogActionByUserIdQuery { Id = userId }).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }
    }
}
