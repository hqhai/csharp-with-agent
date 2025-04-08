// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Api.Controllers.Admin
{
    using System.Net;
    using Asp.Versioning;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Constants;
    using Fsel.Identity.Application.Queries.CompetitionEventsQuery;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Models.ShareModels;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;

    [ApiVersion(ApiSettings.APIVersion1)]
    [ApiVersion(ApiSettings.APIVersion1i1)]
    [Route(Settings.APIDefaultRoute + "/admin/report-event")]
    [Common.Attributes.Permission(roles: new string[] { nameof(EnumRole.Admin), nameof(EnumRole.EducationDepartment), nameof(EnumRole.EducationDivision), nameof(EnumRole.DepartmentAdmin) })]
    [ApiController]
    public class ReportEventController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ReportEventController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Dashboard Event Config
        /// </summary>
        [HttpGet("dashboard-event-config")]
        [ProducesResponseType(typeof(MethodResult<DashboardEventConfig>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetDashboardEventConfig()
        {
            MethodResult<DashboardEventConfig> queryResult = await _mediator.Send(new GetDashboardEventConfigQuery()).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }
    }
}
