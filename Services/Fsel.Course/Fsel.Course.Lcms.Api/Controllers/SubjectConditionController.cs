// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lcms.Api.Controllers
{
    using System.Net;
    using Asp.Versioning;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Constants;
    using Fsel.Course.Application.Commands.SubjectConditionCmd;
    using Fsel.Course.Application.Queries.SubjectConditionQuery;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;

    [ApiVersion(ApiSettings.APIVersion1)]
    [Route(Settings.APIDefaultRoute + "/subject-condition")]
    [ApiController]
    [Common.Attributes.Permission(role: nameof(EnumRole.MasterAdmin))]
    public class SubjectConditionController : ControllerBase
    {
        private readonly IMediator _mediator;

        public SubjectConditionController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Create Subject Condition
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(MethodResult<IList<SubjectConditionModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> CreateSubjectCondition([FromBody] CreateSubjectConditionCommand command)
        {
            MethodResult<IList<SubjectConditionModel>> methodResult = await _mediator.Send(command).ConfigureAwait(false);
            return methodResult.GetActionResult();
        }

        /// <summary>
        /// Update Subject Condition
        /// </summary>
        [HttpPut]
        [ProducesResponseType(typeof(MethodResult<IList<SubjectConditionModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> UpdateSubjectCondition([FromBody] UpdateSubjectConditionCommand command)
        {
            MethodResult<IList<SubjectConditionModel>> methodResult = await _mediator.Send(command).ConfigureAwait(false);
            return methodResult.GetActionResult();
        }

        /// <summary>
        /// Get Subject Condition
        /// </summary>
        [HttpGet("preview")]
        [ProducesResponseType(typeof(MethodResult<IList<SubjectConditionPreviewModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetSubjectConditionPreview([FromQuery] GetSubjectConditionPreviewQuery query)
        {
            MethodResult<IList<SubjectConditionPreviewModel>> methodResult = await _mediator.Send(query).ConfigureAwait(false);
            return methodResult.GetActionResult();
        }
    }
}
