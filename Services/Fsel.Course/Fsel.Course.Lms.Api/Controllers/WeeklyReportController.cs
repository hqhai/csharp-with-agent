// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Api.Controllers
{
    using System.Net;
    using Asp.Versioning;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Constants;
    using Fsel.Course.Lms.Application.Commands.WeeklyReportCommand;
    using Fsel.Shared.Constants;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;

    [ApiVersion(ApiSettings.APIVersion1i1)]
    [ApiVersion(ApiSettings.APIVersion1)]
    [Route(Settings.APIDefaultRoute + "/weekly-report")]
    [ApiController]
    public class WeeklyReportController : ControllerBase
    {
        private readonly IMediator _mediator;

        public WeeklyReportController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// get video time code ranking
        /// </summary>
        [HttpPost("weekly-report")]
        [ProducesResponseType(typeof(MethodResult<bool>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetVideoTimeCodeResult([FromBody] WeeklyReportCommand query)
        {
            var queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }
    }
}
