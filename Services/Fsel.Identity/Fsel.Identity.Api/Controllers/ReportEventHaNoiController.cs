// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Api.Controllers
{
    using System.Net;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Attributes;
    using Fsel.Common.Constants;
    using Fsel.Course.Domain.Models.EntityModels.ReportEventHaNoi;
    using Fsel.Identity.Application.Queries.ReportEventHaNoiQuery;
    using Fsel.Identity.Domain.Models.EntityModels.ReportEventHaNoi;
    using Fsel.Shared.Attributes;
    using Fsel.Shared.Constants;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;

    [ApiVersions(ApiSettings.APIVersion1)]
    [Route(Settings.APIDefaultRoute + "/admin/report-event-hanoi")]
    [ApiController]
    public class ReportEventHaNoiController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ReportEventHaNoiController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Get 
        /// </summary>
        [ServerCache(CacheSettings.TimeCache.ThreeHour)]
        [HttpPost("report-attendance-overall")]
        [ProducesResponseType(typeof(MethodResult<OverallStudentModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> ReportAttendanceOverall([FromBody] ReportAttendanceForCityQuery query)
        {
            var queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        [ServerCache(CacheSettings.TimeCache.ThreeHour)]
        [HttpPost("report-attendance-graph")]
        [ProducesResponseType(typeof(MethodResult<IList<NumberStudentLearnOnSystemModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> ReportAttendanceGraph([FromBody] ReportAttendanceGraphQuery query)
        {
            var queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        [ServerCache(CacheSettings.TimeCache.ThreeHour)]
        [HttpPost("report-attendance-table")]
        [ProducesResponseType(typeof(MethodResult<IList<SummaryDataOnCityModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> ReportAttendanceTable([FromBody] ReportAttendanceTableQuery query)
        {
            var queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }
    }
}
