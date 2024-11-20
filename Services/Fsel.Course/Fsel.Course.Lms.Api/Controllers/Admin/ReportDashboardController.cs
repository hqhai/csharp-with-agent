// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Api.Controllers.Admin
{
    using System.Net;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Constants;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Attributes;
    using Fsel.Course.Lms.Application.Queries.ReportDashboardQuery;
    using Fsel.Course.Domain.Models.QueryModels.ReportDashboard;
    using Fsel.Shared.Enums;

    [ApiVersions(ApiSettings.APIVersion1)]
    [Route(Settings.APIDefaultRoute + "/report-dashboard")]
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

    }
}
