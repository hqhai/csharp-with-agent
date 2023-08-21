// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Api.Controllers.Student
{
    using Fsel.Common.ActionResults;
    using Fsel.Common.Constants;
    using Fsel.Core.Base.BaseModels;
    using Fsel.System.Application.Commands.QuestBoardStudentCmd;
    using Fsel.System.Application.Queries.QuestBoardStudentQuery;
    using Fsel.System.Domain.Models.EntityModels;
    using global::System.Net;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;

    [ApiVersion(Settings.APIVersion)]
    [Route(Settings.APIDefaultRoute + "/student/quest-board")]
    [ApiController]
    public class QuestBoardController : ControllerBase
    {
        private readonly IMediator _mediator;

        public QuestBoardController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Search Quest Board by Student
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(MethodResult<PagingItemsModel<QuestBoardByStudentModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Search([FromQuery] SearchQuestBoardByStudentQuery query)
        {
            var queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// QuestBoard Reward Student
        /// </summary>
        [HttpPost("reward-student/{id}")]
        [ProducesResponseType(typeof(MethodResult<PagingItemsModel<bool>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> QuestBoardRewardStudent([FromRoute] Guid id)
        {
            var queryResult = await _mediator.Send(new QuestRewardCommand { Id = id }).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Taking Mission Student
        /// </summary>
        [HttpPost("taking-mission/{id}")]
        [ProducesResponseType(typeof(MethodResult<PagingItemsModel<bool>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> TakingMissionStudent([FromRoute] Guid id)
        {
            var queryResult = await _mediator.Send(new TakingMissionCommand { Id = id }).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }
    }
}
