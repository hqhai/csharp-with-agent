// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.ExamPractice.Api.Controllers
{
    using System.Net;
    using Asp.Versioning;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Constants;
    using Fsel.Common.Models;
    using Fsel.ExamPractice.Application.Queries.CategoryQuery;
    using Fsel.ExamPractice.Domain.Models.EntityModels.ExamPractices;
    using Fsel.Shared.Constants;
    using MediatR;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;

    [ApiVersion(ApiSettings.APIVersion1)]
    [ApiVersion(ApiSettings.APIVersion1i1)]
    [Route(Settings.APIDefaultRoute + "/category")]
    [ApiController]
    [AllowAnonymous]
    public class CategoryController : ControllerBase
    {
        private readonly IMediator _mediator;

        public CategoryController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Get Category
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(MethodResult<IList<EnumModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [ApiVersion(ApiSettings.APIVersion1)]
        public async Task<IActionResult> Get([FromQuery] GetEnumQuery query)
        {
            MethodResult<IList<EnumModel>> queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Get Category
        /// </summary>
        [HttpGet("exam-practice-subtypes")]
        [ProducesResponseType(typeof(MethodResult<object>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [ApiVersion(ApiSettings.APIVersion1)]
        public async Task<IActionResult> GetExamPracticeSubType([FromQuery] GetExamPracticeSubTypesQuery query)
        {
            MethodResult<object> queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Get Tags
        /// </summary>
        [HttpGet("exam-practice-tags")]
        [ProducesResponseType(typeof(MethodResult<IList<ExamPracticeTagModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [ApiVersion(ApiSettings.APIVersion1)]
        public async Task<IActionResult> GetExamPracticeTags([FromQuery] GetTagExamPracticeQuery query)
        {
            MethodResult<IList<ExamPracticeTagModel>> queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }
    }
}
