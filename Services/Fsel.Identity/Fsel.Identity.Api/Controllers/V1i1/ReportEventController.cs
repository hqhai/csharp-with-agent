// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Api.Controllers.V1i1
{
    using System.Net;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Attributes;
    using Fsel.Common.Constants;
    using Fsel.Identity.Application.Queries.ReportEventHaNoiQuery;
    using Fsel.Identity.Domain.Models.EntityModels.ReportEventHaNoi;
    using Fsel.Shared.Attributes;
    using Fsel.Shared.Constants;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;

    [ApiVersions(ApiSettings.APIVersion1i1)]
    [Route(Settings.APIDefaultRoute + "/admin/report-event")]
    [ApiController]
    [Permission]
    public class ReportEventController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ReportEventController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Get 
        /// </summary>
        [ServerCache(CacheSettings.TimeCache.ThreeHour)]
        [HttpPost("attendance-detail")]
        [ProducesResponseType(typeof(MethodResult<IList<SummaryDataOnCityModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> AttendanceDetail([FromBody] AttendanceDetailQuery query)
        {
            var queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Get 
        /// </summary>
        [ServerCache(CacheSettings.TimeCache.ThreeHour)]
        [HttpPost("attendance-summary")]
        [ProducesResponseType(typeof(MethodResult<IList<SummaryDataOnCityModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> AttendanceSummaryQuery([FromBody] AttendanceSummaryQuery query)
        {
            var queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Get 
        /// </summary>
        [ServerCache(CacheSettings.TimeCache.ThreeHour)]
        [HttpPost("attendance-chart")]
        [ProducesResponseType(typeof(MethodResult<IList<NumberStudentLearnOnSystemModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> AttendanceChart([FromBody] AttendanceChartQuery query)
        {
            var queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }
    }
}
