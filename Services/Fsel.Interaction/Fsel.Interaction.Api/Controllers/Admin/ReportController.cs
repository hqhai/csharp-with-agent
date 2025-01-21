// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Api.Controllers.Admin
{
    using System.Net;
    using Asp.Versioning;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Attributes;
    using Fsel.Common.Constants;
    using Fsel.Interaction.Application.Queries.ExportReportQuery;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;

    [ApiVersion(ApiSettings.APIVersion1)]
    [ApiVersion(ApiSettings.APIVersion1i1)]
    [Route(Settings.APIDefaultRoute + "/admin/report")]
    [ApiController]
    [Permission(role: nameof(EnumRole.Admin))]
    public class ReportController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ReportController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Export Report Time Report
        /// </summary>
        [HttpGet("export-survey-question-event")]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Export([FromQuery] ExportReportSurveyQuestionEventQuery query)
        {
            MethodResult<Stream> commandResult = await _mediator.Send(query).ConfigureAwait(false);
            if (!commandResult.IsOK || commandResult.Result == null)
            {
                return commandResult.GetActionResult();
            }
            return File(commandResult.Result, Settings.Excels.ContentType, "Export_Survey_Question_Event.xlsx");
        }

        /// <summary>
        /// Export Report Time Report
        /// </summary>
        [HttpGet("export-survey-question-event-school")]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Export([FromQuery] ExportReportSurveyQuestionEventSchoolQuery query)
        {
            MethodResult<Stream> commandResult = await _mediator.Send(query).ConfigureAwait(false);
            if (!commandResult.IsOK || commandResult.Result == null)
            {
                return commandResult.GetActionResult();
            }
            return File(commandResult.Result, Settings.Excels.ContentType, "Export_Survey_Question_Event_School.xlsx");
        }
    }
}
