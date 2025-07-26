// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Api.Controllers
{
    using System.Net;
    using Asp.Versioning;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Constants;
    using Fsel.Interaction.Application.Queries.CustomerSurveyQuery;
    using Fsel.Interaction.Application.Queries.SurveyQuestionQuery;
    using Fsel.Interaction.Domain.Models.EntityModels;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;

    [ApiVersion(ApiSettings.APIVersion1)]
    [ApiVersion(ApiSettings.APIVersion1i1)]
    [Route(Settings.APIDefaultRoute + "/surveyQuestion")]
    [ApiController]
    [Common.Attributes.Permission(role: nameof(EnumRole.Student))]
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
        public async Task<IActionResult> GetAll([FromQuery] GetAllSurveyQuestionQuery query)
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
        public async Task<IActionResult> SurveyQuestionsByUserId([FromRoute] Guid userId)
        {
            MethodResult<IList<SurveyQuestionInfoModel>> queryResult = await _mediator.Send(new GetSurveyQuestionsByUserIdQuery { Id = userId }).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Get list Survey Question by ids
        /// </summary>
        [HttpGet("get-questions-survey-pt")]
        [ProducesResponseType(typeof(MethodResult<SurveyConfigModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetSurveyQuestionBySurveyFormType([FromQuery] GetSurveyPTQuery query)
        {
            var queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }
    }
}
