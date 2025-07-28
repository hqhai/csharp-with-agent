// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Api.Controllers.Admin
{
    using System.Net;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Attributes;
    using Fsel.Common.Constants;
    using Fsel.Course.Domain.Models.EntityModels.BaseChartModels;
    using Fsel.Course.Domain.Models.QueryModels.ReportDashboard;
    using Fsel.Course.Lms.Application.Queries.ReportDashboardQuery;
    using Fsel.Shared.Attributes;
    using Fsel.Shared.Constants;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;

    [ApiVersions(ApiSettings.APIVersion1)]
    [Route(Settings.APIDefaultRoute + "/admin/report-dashboard")]
    [ApiController]
    public class ReportDashboard : ControllerBase
    {
        private readonly IMediator _mediator;

        public ReportDashboard(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Search Course
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(MethodResult<ReportPTResultModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [Permission(DashboardManagementByAdminSchool.ViewPTResultsReport)]
        public async Task<IActionResult> ReportDashboardPT([FromQuery] GetReportPTResultQuery query)
        {
            MethodResult<ReportPTResultModel> queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Get Learning Result Statistical
        /// </summary>
        [HttpGet("learning-result-statistical")]
        [ProducesResponseType(typeof(MethodResult<DashBoardLearningResultModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [Permission(DashboardManagementByAdminSchool.ViewLearningResultsReport)]
        public async Task<IActionResult> ReportLearningResult([FromQuery] GetReportLearningResultQuery query)
        {
            MethodResult<DashBoardLearningResultModel> queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Get Student Learning Result Statistical
        /// </summary>
        [HttpGet("student-learning-statistical")]
        [ProducesResponseType(typeof(MethodResult<StackBarChartsModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [Permission(DashboardManagementByAdminSchool.ViewLearningResultsReport)]
        public async Task<IActionResult> ReportLearningResultStack([FromQuery] GetReportLearningResultStackQuery query)
        {
            MethodResult<StackBarChartsModel> queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Get Learning Progress Statistical
        /// </summary>
        [HttpGet("learning-progress-statistical")]
        [ProducesResponseType(typeof(MethodResult<DashBoardLearningProgressModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [Permission(DashboardManagementByAdminSchool.ViewLearningProgressReport)]
        public async Task<IActionResult> ReportLearningProgress([FromQuery] GetReportLearningProgressQuery query)
        {
            MethodResult<DashBoardLearningProgressModel> queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Get Student Learning Result Statistical
        /// </summary>
        [HttpGet("student-learning-progress-statistical")]
        [ProducesResponseType(typeof(MethodResult<StackBarChartsModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [Permission(DashboardManagementByAdminSchool.ViewLearningProgressReport)]
        public async Task<IActionResult> ReportLearningProgressStack([FromQuery] GetReportLearningProgressStackQuery query)
        {
            MethodResult<StackBarChartsModel> queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Get Diligence Report
        /// </summary>
        [HttpGet("student-diligence")]
        [ProducesResponseType(typeof(MethodResult<DashBoardDiligenceModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [Permission(DashboardManagementByAdminSchool.ViewAttendanceReport)]
        public async Task<IActionResult> ReportDashboardDiligence([FromQuery] GetReportDiligenceResultQuery query)
        {
            var queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }
    }
}
