// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Api.Controllers.Admin
{
    using Asp.Versioning;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Attributes;
    using Fsel.Identity.Application.Queries.CompetitionEventsQuery;
    using Fsel.Identity.Domain.Models.EntityModels;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Models.ShareModels.EntityModels;
    using System.Net;
    using Microsoft.AspNetCore.Mvc;
    using MediatR;
    using Fsel.Common.Constants;

    [ApiVersion(ApiSettings.APIVersion1)]
    [ApiVersion(ApiSettings.APIVersion1i1)]
    [Route(Settings.APIDefaultRoute + "/admin/other")]
    [ApiController]
    [Permission(role: nameof(EnumRole.Admin))]
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
        [HttpGet("report-competition-event")]
        [ProducesResponseType(typeof(MethodResult<IList<ReportCompetitionEventModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> ReportCompetitionEvent([FromQuery] GetReportCompetitionEventsQuery query)
        {
            var commandResult = await _mediator.Send(query).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// RetakeCourse School
        /// </summary>
        [HttpGet("report-competition-event-school")]
        [ProducesResponseType(typeof(MethodResult<IList<ReportCompetitionEventModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> ReportCompetitionEvent([FromQuery] GetReportCompetitionEventSchoolsQuery query)
        {
            var commandResult = await _mediator.Send(query).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Get Student Event Registration
        /// </summary>
        [HttpGet("get-student-event-registrations")]
        [ProducesResponseType(typeof(MethodResult<IList<EventRegistrationModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Get([FromQuery] GetStudentEventRegistrationsQuery query)
        {
            var commandResult = await _mediator.Send(query).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }
    }
}
