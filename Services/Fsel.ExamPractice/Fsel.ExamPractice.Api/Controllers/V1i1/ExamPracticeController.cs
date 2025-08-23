// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.ExamPractice.Api.Controllers.V1i1
{
    using System.Net;
    using Asp.Versioning;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Constants;
    using Fsel.ExamPractice.Application.Commands.ExamPracticeCmd.V1i1;
    using Fsel.ExamPractice.Application.Queries.ExamPracticeQuery.V1i1;
    using Fsel.ExamPractice.Domain.Models.EntityModels.ExamPractices;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;

    [ApiVersion(ApiSettings.APIVersion1i1)]
    [Route(Settings.APIDefaultRoute + "/exam-practice")]
    [Common.Attributes.Permission(role: nameof(EnumRole.MasterAdmin))]
    [ApiController]
    public class ExamPracticeController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ExamPracticeController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Create a ExamPractice
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(MethodResult<ExamPracticeModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Create([FromBody] CreateExamPracticeCommand command)
        {
            MethodResult<ExamPracticeModel> queryResult = await _mediator.Send(command).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Update a ExamPractice
        /// </summary>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(MethodResult<ExamPracticeModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] UpdateExamPracticeCommand command)
        {
            ArgumentNullException.ThrowIfNull(command);

            command.Id = id;
            MethodResult<ExamPracticeModel> methodResult = await _mediator.Send(command).ConfigureAwait(false);
            return methodResult.GetActionResult();
        }

        /// <summary>
        /// Get a ExamPractice by OriginalId
        /// </summary>
        [HttpGet("original/{originalId}")]
        [ProducesResponseType(typeof(MethodResult<ExamPracticeModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Get([FromRoute] Guid originalId)
        {
            MethodResult<ExamPracticeModel> queryResult = await _mediator.Send(new GetExamPracticeQuery { OriginalId = originalId }).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }
    }
}
