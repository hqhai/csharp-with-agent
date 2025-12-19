// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.ExamPractice.Api.Controllers
{
    using System.Net;
    using Asp.Versioning;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Constants;
    using Fsel.Core.Base.BaseModels;
    using Fsel.ExamPractice.Application.Commands.ExamPracticeCmd;
    using Fsel.ExamPractice.Application.Queries.ExamPracticeQuery;
    using Fsel.ExamPractice.Domain.Models.EntityModels.ExamPractices;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;

    [ApiVersion(ApiSettings.APIVersion1)]
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
        [ApiVersion(ApiSettings.APIVersion1)]
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
        [ApiVersion(ApiSettings.APIVersion1)]
        public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] UpdateExamPracticeCommand command)
        {
            command.Id = id;
            MethodResult<ExamPracticeModel> queryResult = await _mediator.Send(command).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Change Status ExamPractice
        /// </summary>
        [HttpPut("change-status")]
        [ProducesResponseType(typeof(MethodResult<ExamPracticeModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [ApiVersion(ApiSettings.APIVersion1)]
        public async Task<IActionResult> ChangeStatus([FromBody] ChangeStatusCommand command)
        {
            MethodResult<ExamPracticeModel> queryResult = await _mediator.Send(command).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Get a ExamPractice
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(MethodResult<ExamPracticeModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [ApiVersion(ApiSettings.APIVersion1)]
        public async Task<IActionResult> Get([FromRoute] Guid id)
        {
            MethodResult<ExamPracticeModel> queryResult = await _mediator.Send(new GetExamPracticeQuery { Id = id }).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Search a ExamPractice
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(MethodResult<PagingItemsModel<ExamPracticeSearchModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [ApiVersion(ApiSettings.APIVersion1)]
        public async Task<IActionResult> Get([FromQuery] SearchExamPracticeQuery query)
        {
            MethodResult<PagingItemsModel<ExamPracticeSearchModel>> queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Get ExamPractice Histories
        /// </summary>
        [HttpGet("histories")]
        [ProducesResponseType(typeof(MethodResult<PagingItemsModel<ExamPracticeModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetUnitHistory([FromQuery] GetHistoryExamPracticeQuery query)
        {
            var queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Change Status ExamPractice
        /// </summary>
        [HttpPut("archive/{id}")]
        [ProducesResponseType(typeof(MethodResult<bool>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [ApiVersion(ApiSettings.APIVersion1)]
        public async Task<IActionResult> ArchiveExamPratice([FromRoute] Guid id)
        {
            MethodResult<bool> queryResult = await _mediator.Send(new ArchiveExamPraticeCommand { Id = id }).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Change Status ExamPractice
        /// </summary>
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(MethodResult<bool>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [ApiVersion(ApiSettings.APIVersion1)]
        public async Task<IActionResult> DeleteExamPratice([FromRoute] Guid id)
        {
            MethodResult<bool> queryResult = await _mediator.Send(new DeleteExamPraticeCommand { Id = id }).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }
    }
}
