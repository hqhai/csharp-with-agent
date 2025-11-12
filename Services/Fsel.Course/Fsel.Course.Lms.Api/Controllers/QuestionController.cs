// Copyright (c) Atlantic. All rights reserved.
// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Api.Controllers
{
    using System.Net;
    using Asp.Versioning;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Attributes;
    using Fsel.Common.Constants;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Lms.Application.Commands.QuestionExplanationErrorCmd;
    using Fsel.Course.Lms.Application.Queries.QuestionQuery;
    using Fsel.Shared.Attributes;
    using Fsel.Shared.Constants;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;

    [ApiVersions(ApiSettings.APIVersion1)]
    [ApiController]
    [Route(Settings.APIDefaultRoute + "/question")]
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
        [MapToApiVersion(ApiSettings.APIVersion1)]
        [MapToApiVersion(ApiSettings.APIVersion1i1)]
        [EncryptResponse]
        [HttpGet]
        [ProducesResponseType(typeof(MethodResult<IList<QuestionModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Get([FromQuery] GetQuestionByIdsQuery query)
        {
            MethodResult<IList<QuestionModel>> queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Get Question
        /// </summary>
        [HttpPost("error-report-explanation-question")]
        [ProducesResponseType(typeof(MethodResult<bool>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> CreateErrorReportExplanation([FromBody] CreateQuestionExplanationErrorCommand command)
        {
            MethodResult<bool> commandResult = await _mediator.Send(command).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Get SubQuestion
        /// </summary>
        [HttpGet("sub-questions")]
        [ProducesResponseType(typeof(MethodResult<IList<SubQuestionModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetSubQuestion([FromQuery] GetSubQuestionsByMockTestQuery query)
        {
            MethodResult<IList<SubQuestionModel>> queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Get Question
        /// </summary>
        [EncryptResponse]
        [HttpGet("get-pt-question-by-ids")]
        [ProducesResponseType(typeof(MethodResult<IList<QuestionModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Get([FromQuery] GetPtQuestionByIdsQuery query)
        {
            var queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }
    }
}
