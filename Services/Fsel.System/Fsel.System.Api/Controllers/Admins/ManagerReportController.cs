// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Api.Controllers.Admins
{
    using Fsel.Common.ActionResults;
    using Fsel.Common.Attributes;
    using Fsel.Common.Constants;
    using Fsel.Shared.Attributes;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Enums;
    using Fsel.System.Application.Commands.ManagerReportCmd;
    using Fsel.System.Application.Queries.ManagerReportQuery;
    using Fsel.System.Application.Queries.Reports;
    using Fsel.System.Domain.Models.EntityModels;
    using Fsel.System.Domain.Models.EntityModels.ManagerReportModels;
    using global::System.Net;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;

    [ApiVersions(ApiSettings.APIVersion1)]
    [Route(Settings.APIDefaultRoute + "/manager-report/admin")]
    [Permission(roles: new string[] { nameof(EnumRole.Admin), nameof(EnumRole.AdminSchool) })]
    [ApiController]
    public class ManagerReportController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ManagerReportController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// search
        /// </summary>
        [HttpGet("search-report-student-assiduity")]
        [ProducesResponseType(typeof(MethodResult<SearchReportStudentAssiduityModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Get([FromQuery] SearchReportAssiduityQuery query)
        {
            var queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Overall Report PlacementTest
        /// </summary>
        [HttpGet("export-report-student-assiduity")]
        [ProducesResponseType(typeof(MethodResult<Stream>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Get([FromQuery] ExportFileExcelReportStudentAssiduityCommand command)
        {
            var commandResult = await _mediator.Send(command).ConfigureAwait(false);
            if (!commandResult.IsOK || commandResult.Result == null)
            {
                return commandResult.GetActionResult();
            }
            return File(commandResult.Result, Settings.Excels.ContentType, "Export_Report_StudentAssiduity.xlsx");
        }

        /// <summary>
        /// aggregate data students in event
        /// </summary>
        [HttpPost("aggregate-data-students-in-event")]
        [ProducesResponseType(typeof(MethodResult<IList<AggregateDataStudentsInEventModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> AggregateDataStudentsInEvent([FromBody] AggregateDataStudentsInEventQuery query)
        {
            var queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }
    }
}
