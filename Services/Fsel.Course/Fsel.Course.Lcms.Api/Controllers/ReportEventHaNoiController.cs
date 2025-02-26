// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lcms.Api.Controllers
{
    using System.Net;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Attributes;
    using Fsel.Common.Constants;
    using Fsel.Course.Application.Queries.ReportEventHaNoiQuery;
    using Fsel.Course.Domain.Models.EntityModels.ReportEventHaNoi;
    using Fsel.Course.Lms.Application.Queries.ReportEventHaNoiQuery;
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
        [ServerCache(CacheSettings.TimeCache.OneHour)]
        [HttpPost("evaluat-input-result")]
        [ProducesResponseType(typeof(MethodResult<EvaluateInputResultModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetListUnitByCourse([FromBody] EvaluateInputResultCityQuery query)
        {
            var queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Get 
        /// </summary>
        [ServerCache(CacheSettings.TimeCache.OneHour)]
        [HttpPost("school-summary")]
        [ProducesResponseType(typeof(MethodResult<EvaluateInputResultModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> SchoolSummary([FromBody] SchoolSummaryQuery query)
        {
            var queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Get 
        /// </summary>
        [ServerCache(CacheSettings.TimeCache.OneHour)]
        [HttpPost("learning-progress")]
        [ProducesResponseType(typeof(MethodResult<LearningProgressModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> LearningProgress([FromBody] LearningProgressQuery query)
        {
            var queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Get 
        /// </summary>
        [ServerCache(CacheSettings.TimeCache.OneHour)]
        [HttpPost("learning-quality")]
        [ProducesResponseType(typeof(MethodResult<LearningQualityModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> LearningQuality([FromBody] LearningQualityQuery query)
        {
            var queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Get 
        /// </summary>
        [ServerCache(CacheSettings.TimeCache.OneHour)]
        [HttpPost("report-attendance-overall")]
        [ProducesResponseType(typeof(MethodResult<OverallStudentModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> ReportAttendanceOverall([FromBody] ReportAttendanceForCityQuery query)
        {
            var queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        [ServerCache(CacheSettings.TimeCache.OneHour)]
        [HttpPost("report-attendance-graph")]
        [ProducesResponseType(typeof(MethodResult<IList<NumberStudentLearnOnSystemModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> ReportAttendanceGraph([FromBody] ReportAttendanceGraphQuery query)
        {
            var queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        [ServerCache(CacheSettings.TimeCache.OneHour)]
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
