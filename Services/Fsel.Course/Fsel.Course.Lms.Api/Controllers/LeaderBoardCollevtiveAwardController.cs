// Copyright (c) Atlantic. All rights reserved.

using System.Net;
using Fsel.Common.ActionResults;
using Fsel.Common.Attributes;
using Fsel.Common.Constants;
using Fsel.Core.Base;
using Fsel.Core.Base.BaseModels;
using Fsel.Course.Domain.IRepositories;
using Fsel.Course.Domain.Models.EntityModels;
using Fsel.Course.Domain.Models.QueryModels.LeaderBoardRewards;
using Fsel.Course.Lms.Application.Queries.LeaderBoardCollectiveAwardQuery;
using Fsel.Shared.Attributes;
using Fsel.Shared.Constants;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Fsel.Course.Lms.Api.Controllers
{
    [ApiVersions(ApiSettings.APIVersion1)]
    [Route(Settings.APIDefaultRoute + "/leader-board-collective")]
    [ApiController]
    public class LeaderBoardCollevtiveAwardController : BaseController
    {
        private readonly IMediator _mediator;
        private readonly ICourseRepository _courseRepository;

        public LeaderBoardCollevtiveAwardController(IMediator mediator, ICourseRepository courseRepository)
        {
            _mediator = mediator;
            _courseRepository = courseRepository;
        }
        /// <summary>
        /// Execute-list-query
        /// </summary>
        [HttpGet("execute-query")]
        [ProducesResponseType(typeof(MethodResult<CourseModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [Permission]
        public async Task<IActionResult> Execute([FromQuery] BaseQueryModel query)
        {
            SetQuery(query);
            var result = await _courseRepository.GetResultAsync<CourseModel>(query);
            return result.GetActionResult();
        }


        /// <summary>
        /// Get LeaderBoardCollevtive award
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(MethodResult<IList<LeaderBoardCollectiveAwardModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetLessonScore([FromQuery] GetLeaderBoardCollectiveQuery query)
        {
            SetQuery(query);
            MethodResult<PagingItemsModel<LeaderBoardCollectiveAwardModel>> queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }
    }
}
