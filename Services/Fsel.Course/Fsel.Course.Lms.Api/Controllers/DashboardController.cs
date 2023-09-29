// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Api.Controllers
{
    using System.Net;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Constants;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Lms.Application.Queries.DashboardQuery;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;

    [ApiVersion(Settings.APIVersion)]
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
        [HttpGet("leader-board/{id}")]
        [ProducesResponseType(typeof(MethodResult<LeaderBoardSearchModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetLeaderBoard(Guid id)
        {
            MethodResult<LeaderBoardSearchModel> queryResult = await _mediator.Send(new GetLeaderBoardQuery { UserId = id }).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Get Lesson Overview
        /// </summary>
        [HttpGet("lesson-overview")]
        [ProducesResponseType(typeof(MethodResult<LessonOverviewModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetLessonOverview()
        {
            MethodResult<LessonOverviewModel> queryResult = await _mediator.Send(new GetLessonOverviewQuery()).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }
    }
}
