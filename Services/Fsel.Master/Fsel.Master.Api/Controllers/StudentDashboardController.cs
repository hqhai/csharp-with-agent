// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Master.Api.Controllers
{
    using System.Collections.Generic;
    using System.Net;
    using System.Threading.Tasks;
    using Asp.Versioning;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Constants;
    using Fsel.Master.Domain.Models.EntityModels.StudentDashboard;
    using Fsel.Master.Domain.Models.QueryModels.StudentDashboard;
    using Fsel.Shared.Constants;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;

    [ApiVersion(ApiSettings.APIVersion1)]
    [ApiVersion(ApiSettings.APIVersion1i1)]
    [Route(Settings.APIDefaultRoute + "/progress-metric/student-dashboard")]
    [ApiController]
    public class StudentDashboardController : ControllerBase
    {
        private readonly IMediator _mediator;

        public StudentDashboardController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Get student dashboard summary - summary cards
        /// </summary>
        [HttpGet("summary")]
        [ProducesResponseType(typeof(MethodResult<StudentDashboardSummaryModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetSummary([FromQuery] GetStudentDashboardSummaryQuery query)
        {
            var queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Get entrance exam overview - pie chart data
        /// </summary>
        [HttpGet("charts/entrance-exam-overview")]
        [ProducesResponseType(typeof(MethodResult<EntranceExamOverviewModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetEntranceExamOverview([FromQuery] GetEntranceExamOverviewQuery query)
        {
            var queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Get province entrance exam - stacked bar chart data
        /// </summary>
        [HttpGet("charts/province-entrance-exam")]
        [ProducesResponseType(typeof(MethodResult<IList<ProvinceEntranceExamModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetProvinceEntranceExam([FromQuery] GetProvinceEntranceExamQuery query)
        {
            var queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Get weekly progress - line chart data
        /// </summary>
        [HttpGet("charts/weekly-progress")]
        [ProducesResponseType(typeof(MethodResult<IList<WeeklyProgressModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetWeeklyProgress([FromQuery] GetWeeklyProgressQuery query)
        {
            var queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Get course completion - KPI + Top 10 students
        /// </summary>
        [HttpGet("course-completion")]
        [ProducesResponseType(typeof(MethodResult<CourseCompletionResponseModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetCourseCompletion([FromQuery] GetCourseCompletionQuery query)
        {
            var queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }
    }
}
