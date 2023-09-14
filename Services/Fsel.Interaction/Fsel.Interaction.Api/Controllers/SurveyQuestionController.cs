// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Api.Controllers
{
    using System.Net;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Constants;
    using Fsel.Interaction.Application.Queries.SurveyQuestionQuery;
    using Fsel.Interaction.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;

    [ApiVersion(Settings.APIVersion)]
    [Route(Settings.APIDefaultRoute + "/surveyQuestion")]
    [ApiController]
    public class SurveyQuestionController : ControllerBase
    {
        private readonly IMediator _mediator;

        public SurveyQuestionController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// GetAll Survey Question
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(MethodResult<IList<SurveyQuestionModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetAll([FromQuery] GetAllSurveyQuestQuery query)
        {
            MethodResult<IList<SurveyQuestionModel>> queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Get list Survey Question by ids
        /// </summary>
        [HttpGet("get-list-question-by-ids")]
        [ProducesResponseType(typeof(MethodResult<IList<SurveyQuestionModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetListQuestionByIds([FromQuery] GetListQuestionByIdsQuery query)
        {
            MethodResult<IList<SurveyQuestionModel>> queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Get SurveyQuestions By UserId
        /// </summary>
        [HttpGet("{userId}")]
        [ProducesResponseType(typeof(MethodResult<IList<SurveyQuestionInfoModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> SurveyQuestionsByUserId([FromRoute] string userId)
        {
            MethodResult<IList<SurveyQuestionInfoModel>> queryResult = await _mediator.Send(new GetSurveyQuestionsByUserIdQuery { Id = userId }).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }
    }
}
