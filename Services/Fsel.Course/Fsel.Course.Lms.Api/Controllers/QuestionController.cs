// Copyright (c) Atlantic. All rights reserved.
// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Api.Controllers
{
    using System.Net;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Constants;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Lms.Application.Queries.QuestionQuery;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Attributes;
    using Asp.Versioning;
    using Fsel.Common.Attributes;

    [ApiVersions(ApiSettings.APIVersion1)]
    [ApiController]
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
    }
}
