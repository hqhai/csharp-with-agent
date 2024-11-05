// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Api.Controllers.Admin
{
    using System.Net;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Constants;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Lms.Application.Commands.ManagerReportCmd;
    using Fsel.Course.Lms.Application.Queries.ManagerReportQuery;
    using Fsel.Shared.Attributes;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Models.ShareModels.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;

    [ApiVersions(ApiSettings.APIVersion1)]
    [Route(Settings.APIDefaultRoute + "/manager-report/admin")]
    [ApiController]
    public class ManagerReportController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ManagerReportController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Overall Report PlacementTest
        /// </summary>
        [HttpGet("overall-report-placement-test")]
        [ProducesResponseType(typeof(MethodResult<OverallReportPlacementTestModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [Common.Attributes.Permission(roles: new string[] { nameof(EnumRole.Admin), nameof(EnumRole.AdminSchool) })]
        public async Task<IActionResult> Get([FromQuery] GetOverallReportPlacementTestQuery query)
        {
            var queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// search
        /// </summary>
        [HttpGet("search")]
        [ProducesResponseType(typeof(MethodResult<SearchReportPlacementTestModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [Common.Attributes.Permission(roles: new string[] { nameof(EnumRole.Admin), nameof(EnumRole.AdminSchool) })]
        public async Task<IActionResult> Get([FromQuery] SearchReportPlacementTestQuery query)
        {
            var queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Overall Report PlacementTest
        /// </summary>
        [HttpGet("export-file")]
        [ProducesResponseType(typeof(MethodResult<OverallReportPlacementTestModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [Common.Attributes.Permission(roles: new string[] { nameof(EnumRole.AdminSchool) })]
        public async Task<IActionResult> Get([FromQuery] ExportFileExcelReportPlacementTestCommand command)
        {
            var commandResult = await _mediator.Send(command).ConfigureAwait(false);
            if (!commandResult.IsOK || commandResult.Result == null)
            {
                return commandResult.GetActionResult();
            }
            return File(commandResult.Result, Settings.Excels.ContentType, "Export_Report_PlacementTests.xlsx");
        }
    }
}
