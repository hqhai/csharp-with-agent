// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Api.Controllers.Admin
{
    using System.Net;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Attributes;
    using Fsel.Common.Constants;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Lms.Application.Queries.CourseQuery;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Attributes;
    using Fsel.Course.Lms.Application.Queries.ReportDashboardQuery;
    using Fsel.Course.Domain.Models.QueryModels.ReportDashboard;
    using Fsel.Course.Domain.Models.EntityModels.BaseChartModels;
    using Fsel.Shared.Enums;

    [ApiVersions(ApiSettings.APIVersion1)]
    [Route(Settings.APIDefaultRoute + "/admin/report-dashboard")]
    [Permission(roles: new string[] { nameof(EnumRole.Admin), nameof(EnumRole.AdminSchool) })]
    [ApiController]
    [Common.Attributes.Permission(role: nameof(EnumRole.AdminSchool))]
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
        public async Task<IActionResult> ReportLearningProgressStack([FromQuery] GetReportLearningProgressStackQuery query)
        {
            MethodResult<StackBarChartsModel> queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }
    }
}