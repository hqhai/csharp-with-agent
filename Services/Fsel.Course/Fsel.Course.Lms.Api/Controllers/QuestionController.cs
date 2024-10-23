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
    using Fsel.Course.Lms.Application.Queries.QuestionQuery;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;

    [ApiVersion(ApiSettings.APIVersion1)]
    [ApiVersion(ApiSettings.APIVersion1i1)]
    [Route(Settings.APIDefaultRoute + "/question")]
    [Permission(role: nameof(EnumRole.Student))]
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
        public async Task<IActionResult> GetSubQuestion([FromQuery] GetSubQuestionsByMockTestQuery query)
        {
            MethodResult<IList<SubQuestionModel>> queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }
    }
}
