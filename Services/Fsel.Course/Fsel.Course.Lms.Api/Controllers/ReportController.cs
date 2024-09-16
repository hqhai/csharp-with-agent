// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Api.Controllers
{
    using System.Net;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Constants;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Lms.Application.Queries.OtherFeatureQuery;
    using Fsel.Course.Lms.Application.Queries.Reports;
    using Fsel.Shared.Attributes;
    using Fsel.Shared.Constants;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;

    [ApiVersions(ApiSettings.APIVersion1)]
    [Route(Settings.APIDefaultRoute + "/report")]
    [ApiController]
    public class ReportController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ReportController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Get Overall Report By Student
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(MethodResult<OverallReportModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Get([FromQuery] GetOverallReportByStudentQuery query)
        {
            var queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Expot File ExplanationQuestion
        /// </summary>
        [HttpPost("export-file-explanation-question")]
        [ProducesResponseType(typeof(MethodResult<Stream>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> ExportFile([FromQuery] ExportFileReportExplanationLogQuery query)
        {
            var queryResult = await _mediator.Send(query).ConfigureAwait(false);
            if (!queryResult.IsOK || queryResult.Result == null)
            {
                return queryResult.GetActionResult();
            }
            return File(queryResult.Result, Settings.Excels.ContentType, "export_file_explanation_question.xlsx");
        }
    }
}
