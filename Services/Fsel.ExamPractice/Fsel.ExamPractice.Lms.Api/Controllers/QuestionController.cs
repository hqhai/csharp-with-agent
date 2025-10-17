// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.ExamPractice.Lms.Api.Controllers
{
    using System.Net;
    using Asp.Versioning;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Constants;
    using Fsel.ExamPractice.Domain.Models.EntityModels.Questions;
    using Fsel.ExamPractice.Lms.Application.Queries.QuestionQuery;
    using Fsel.Shared.Attributes;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;

    [ApiVersions(ApiSettings.APIVersion1)]
    [ApiController]
    [Route(Settings.APIDefaultRoute + "/question")]
   [Common.Attributes.Permission(roles: new string[] { nameof(EnumRole.Student), nameof(EnumRole.StudentCampus) })]
    public class QuestionController : ControllerBase
    {
        private readonly IMediator _mediator;

        public QuestionController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Get Question
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(MethodResult<IList<QuestionModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Get([FromQuery] GetQuestionByIdsQuery query)
        {
            MethodResult<IList<QuestionModel>> queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Get SubQuestion
        /// </summary>
        [HttpGet("sub-questions")]
        [ProducesResponseType(typeof(MethodResult<IList<SubQuestionModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetSubQuestion([FromQuery] GetIELTSSubQuestionsQuery query)
        {
            MethodResult<IList<SubQuestionModel>> queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }
    }
}
