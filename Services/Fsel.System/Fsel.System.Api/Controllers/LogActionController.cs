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
    using Asp.Versioning;
    using Fsel.Shared.Constants;

    [ApiVersion(ApiSettings.APIVersion1)][ApiVersion(ApiSettings.APIVersion1i1)]
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
        [ProducesResponseType(typeof(MethodResult<LogActionDaysModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetLogActionsByUserId([FromRoute] Guid userId)
        {
            MethodResult<LogActionDaysModel> commandResult = await _mediator.Send(new GetLogActionByUserIdQuery { Id = userId }).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Get LogActions by UserIds
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(MethodResult<IList<LogActionDaysModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetLogActionsByUserIds([FromBody] IList<Guid> ids)
        {
            MethodResult<IList<LogActionDaysModel>> commandResult = await _mediator.Send(new GetListLogActionByUserIdsQuery { Ids = ids }).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }
    }
}
