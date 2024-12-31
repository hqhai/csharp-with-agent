// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Api.Controllers
{
    using System.Net;
    using Asp.Versioning;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Constants;
    using Fsel.Identity.Application.Commands.OtherCmd;
    using Fsel.Identity.Application.Queries.CompetitionEventsQuery;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Models.ShareModels.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;

    [ApiVersion(ApiSettings.APIVersion1)]
    [ApiVersion(ApiSettings.APIVersion1i1)]
    [Route(Settings.APIDefaultRoute + "/other")]
    [ApiController]
    public class OtherController : ControllerBase
    {
        private readonly IMediator _mediator;

        public OtherController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// RetakeCourse
        /// </summary>
        [HttpGet("retake-course")]
        [ProducesResponseType(typeof(MethodResult<string>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<RedirectResult> RetakeCourse([FromQuery] RetakeCourseResultCommand command)
        {
            var commandResult = await _mediator.Send(command).ConfigureAwait(false);
            return Redirect(commandResult.Result ?? string.Empty);
        }

        /// <summary>
        /// RetakeCourse
        /// </summary>
        [HttpGet("report-competition-event")]
        [ProducesResponseType(typeof(MethodResult<IList<ReportCompetitionEventModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> ReportCompetitionEvent([FromQuery] GetReportCompetitionEventsQuery query)
        {
            var commandResult = await _mediator.Send(query).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }
    }
}
