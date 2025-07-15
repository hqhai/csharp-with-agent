// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Api.Controllers.Admin
{
    using System.Net;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Attributes;
    using Fsel.Common.Constants;
    using Fsel.Core.Base;
    using Fsel.Course.Domain.Models.EntityModels.ManagerReportModels;
    using Fsel.Course.Lms.Application.Commands.ManagerReportCmd;
    using Fsel.Course.Lms.Application.Queries.ManagerReportQuery;
    using Fsel.Shared.Attributes;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Helpers;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;

    [ApiVersions(ApiSettings.APIVersion1)]
    [Route(Settings.APIDefaultRoute + "/manager-report/admin")]
    [ApiController]
    public class ManagerReportController : BaseController
    {
        private readonly IMediator _mediator;

        public ManagerReportController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// search
        /// </summary>
        [HttpGet("search-report-learning-progress")]
        [ProducesResponseType(typeof(MethodResult<SearchReportLearningProgressModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [Permission(ReportManagementByAdminSchool.ViewLearningProgressReport)]
        public async Task<IActionResult> Get([FromQuery] SearchReportLearningProgressQuery query)
        {
            SetQuery(query);
            var queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// search
        /// </summary>
        [HttpGet("search-report-learning-result")]
        [ProducesResponseType(typeof(MethodResult<SearchReportLearningResultModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [Permission(ReportManagementByAdminSchool.ViewLearningResultsReport)]
        public async Task<IActionResult> Get([FromQuery] SearchReportLearningResultQuery query)
        {
            SetQuery(query);
            var queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// search
        /// </summary>
        [HttpGet("search-report-placement-test")]
        [ProducesResponseType(typeof(MethodResult<SearchReportPlacementTestModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [Permission(ReportManagementByAdminSchool.ViewPTResultsReport)]
        public async Task<IActionResult> Get([FromQuery] SearchReportPlacementTestQuery query)
        {
            var queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Overall Report PlacementTest
        /// </summary>
        [HttpGet("export-report-placement-test")]
        [ProducesResponseType(typeof(MethodResult<Stream>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [Permission(ReportManagementByAdminSchool.ViewPTResultsReport)]
        public async Task<IActionResult> Get([FromQuery] ExportFileExcelReportPlacementTestCommand command)
        {
            var commandResult = await _mediator.Send(command).ConfigureAwait(false);
            if (!commandResult.IsOK || commandResult.Result == null)
            {
                return commandResult.GetActionResult();
            }
            return File(commandResult.Result, Settings.Excels.ContentType, $"Export_Report_PlacementTests_{NumberHelper.GenerateCodeNumber(4)}.xlsx");
        }

        /// <summary>
        /// Overall Report PlacementTest
        /// </summary>
        [HttpGet("export-report-learning-progress")]
        [ProducesResponseType(typeof(MethodResult<Stream>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [Permission(ReportManagementByAdminSchool.ViewLearningProgressReport)]
        public async Task<IActionResult> Get([FromQuery] ExportFileExcelReportLearningProgressCommand command)
        {
            SetQuery(command);
            var commandResult = await _mediator.Send(command).ConfigureAwait(false);
            if (!commandResult.IsOK || commandResult.Result == null)
            {
                return commandResult.GetActionResult();
            }
            return File(commandResult.Result, Settings.Excels.ContentType, $"Export_Report_LearningProgress_{command.CourseType}_{NumberHelper.GenerateCodeNumber(4)}.xlsx");
        }

        /// <summary>
        /// Overall Report PlacementTest
        /// </summary>
        [HttpGet("export-report-learning-result")]
        [ProducesResponseType(typeof(MethodResult<Stream>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [Permission(ReportManagementByAdminSchool.ViewLearningResultsReport)]
        public async Task<IActionResult> Get([FromQuery] ExportFileExcelReportLearningResultCommand command)
        {
            SetQuery(command);
            var commandResult = await _mediator.Send(command).ConfigureAwait(false);
            if (!commandResult.IsOK || commandResult.Result == null)
            {
                return commandResult.GetActionResult();
            }
            return File(commandResult.Result, Settings.Excels.ContentType, $"Export_Report_LearningResult_{command.CourseType}_{NumberHelper.GenerateCodeNumber(4)}.xlsx");
        }
    }
}
