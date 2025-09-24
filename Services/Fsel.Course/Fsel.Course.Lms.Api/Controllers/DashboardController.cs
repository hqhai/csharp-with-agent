// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Api.Controllers
{
    using System.Net;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Constants;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Lms.Application.Queries.DashboardQuery;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Enums;
    using Fsel.Course.Lms.Application.Queries.CourseQuery;
    using Fsel.Shared.Attributes;

    [ApiVersions(ApiSettings.APIVersion1)]
    [Route(Settings.APIDefaultRoute + "/dashboard")]
    [ApiController]
    public class DashboardController : ControllerBase
    {
        private readonly IMediator _mediator;

        public DashboardController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Get Leader Board
        /// </summary>
        [HttpGet("leader-board")]
        [ProducesResponseType(typeof(MethodResult<LeaderBoardSearchModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetLeaderBoard()
        {
            MethodResult<LeaderBoardSearchModel> queryResult = await _mediator.Send(new GetLeaderBoardQuery()).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Get Current-Position
        /// </summary>
        [HttpGet("current-student")]
        [ProducesResponseType(typeof(MethodResult<LeaderBoardSearchModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetCurrentStudentUser()
        {
            MethodResult<LeaderBoardSearchModel> queryResult = await _mediator.Send(new GetCurrentPositionQuery()).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Get Lesson Overview
        /// </summary>
        [HttpGet("lesson-overview")]
       [Common.Attributes.Permission(roles: new string[] { nameof(EnumRole.Student), nameof(EnumRole.StudentCampus) })]
        [ProducesResponseType(typeof(MethodResult<LessonOverviewModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetLessonOverview()
        {
            MethodResult<LessonOverviewModel> queryResult = await _mediator.Send(new GetLessonOverviewQuery()).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        [HttpPost("active-course-result")]
        [ProducesResponseType(typeof(MethodResult<List<IList<Guid>>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetActiveCourseResult([FromBody] GetActiveCourseQuery query)
        {
            MethodResult<IList<Guid>> commandResult = await _mediator.Send(query).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }
    }
}
