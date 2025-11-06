// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Api.Controllers.V1i1
{
    using System.Net;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Attributes;
    using Fsel.Common.Constants;
    using Fsel.Course.Domain.Models.EntityModels.DashboardModels;
    using Fsel.Course.Lms.Application.Queries.DashboardQuery.V1i1;
    using Fsel.Shared.Attributes;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;

    [ApiVersions(ApiSettings.APIVersion1i1)]
    [Route(Settings.APIDefaultRoute + "/dashboard")]
    [Permission(role: nameof(EnumRole.Student))]
    [ApiController]
    public class DashboardController : ControllerBase
    {
        private readonly IMediator _mediator;

        public DashboardController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Lấy biểu đồ radar năng lực (competency radar) của học viên.
        /// </summary>
        [HttpGet("competency-radar")]
        [ProducesResponseType(typeof(MethodResult<CompetencyRadarModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetCompetencyRadar([FromQuery] GetCompetencyAssessmentRadarQuery query)
        {
            var commandResult = await _mediator.Send(query).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Lấy biểu đồ radar năng lực (competency radar) của học viên.
        /// </summary>
        [HttpGet("dashboard-home")]
        [ProducesResponseType(typeof(MethodResult<DashboardHomeModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetDashboardHome()
        {
            var commandResult = await _mediator.Send(new GetDashboardHomeQuery()).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Lấy biểu đồ radar năng lực (competency radar) của học viên.
        /// </summary>
        [HttpGet("navigation-target")]
        [ProducesResponseType(typeof(MethodResult<HomeNavigationTargetModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetHomeNavigation()
        {
            var commandResult = await _mediator.Send(new GetHomeNavigationTargetQuery()).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Lấy biểu đồ radar năng lực (competency radar) của học viên.
        /// </summary>
        [HttpGet("overall-home")]
        [ProducesResponseType(typeof(MethodResult<OverallHomeModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetOverallHome()
        {
            var commandResult = await _mediator.Send(new GetOverallHomeQuery()).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }
    }
}
