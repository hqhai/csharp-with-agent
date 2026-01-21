// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Api.Controllers
{
    using System.Net;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Constants;
    using Fsel.Course.Domain.Models.EntityModels.AiPromptManagerModels;
    using Fsel.Course.Lms.Application.Queries.AICriteriaConfigQuery;
    using Fsel.Shared.Attributes;
    using Fsel.Shared.Constants;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;

    [ApiVersions(ApiSettings.APIVersion1)]
    [Route(Settings.APIDefaultRoute + "/ai-criteria-configs")]
    [ApiController]
    public class AICriteriaConfigController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AICriteriaConfigController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Get AI criteria config detail by id
        /// </summary>
        [HttpGet("by-id")]
        [ProducesResponseType(typeof(MethodResult<AICriteriaConfigsModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetById([FromQuery] GetAICriteriaConfigByIdQuery query)
        {
            var methodResult = await _mediator.Send(query).ConfigureAwait(false);
            return methodResult.GetActionResult();
        }

        /// <summary>
        /// Get AI criteria config details by ids (batch)
        /// </summary>
        [HttpPost("by-ids")]
        [ProducesResponseType(typeof(MethodResult<IList<AICriteriaConfigsModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetByIds([FromBody] GetAICriteriaConfigsQuery query)
        {
            var methodResult = await _mediator.Send(query).ConfigureAwait(false);
            return methodResult.GetActionResult();
        }
    }
}
