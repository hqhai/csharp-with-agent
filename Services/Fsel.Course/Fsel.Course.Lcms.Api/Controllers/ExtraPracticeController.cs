// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lcms.Api.Controllers
{
    using System.Net;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Constants;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Course.Application.Commands.ExtraPracticeCmd;
    using Fsel.Course.Application.Queries.ExtraPracticeQuery;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;

    [ApiVersion(Settings.APIVersion)]
    [Route(Settings.APIDefaultRoute + "/extrapractice")]
    [ApiController]
    [Authorize(Roles = nameof(EnumRole.MasterAdmin))]
    public class ExtraPracticeController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ExtraPracticeController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Update Status ExtraPractice
        /// </summary>
        [HttpPost("active")]
        [ProducesResponseType(typeof(MethodResult<bool>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> ActiveExtraPractice([FromBody] UpdateStatusExtraPracticeCommand command)
        {
            MethodResult<bool> queryResult = await _mediator.Send(command).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Create a ExtraPractice
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(MethodResult<ExtraPracticeModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Create([FromBody] CreateExtraPracticeCommand command)
        {
            MethodResult<ExtraPracticeModel> commandResult = await _mediator.Send(command).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Search ExtraPractices
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(MethodResult<PagingItemsModel<ExtraPracticeModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Search([FromQuery] SearchExtraPracticeQuery query)
        {
            MethodResult<PagingItemsModel<ExtraPracticeModel>> queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Update a ExtraPractice
        /// </summary>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(MethodResult<ExtraPracticeModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] UpdateExtraPracticeCommand command)
        {
            ArgumentNullException.ThrowIfNull(command);
            command.Id = id;
            MethodResult<ExtraPracticeModel> commandResult = await _mediator.Send(command).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Get Detail ExtraPractice
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(MethodResult<ExtraPracticeModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Get([FromRoute] Guid id)
        {
            MethodResult<ExtraPracticeModel> commandResult = await _mediator.Send(new GetExtraPracticeQuery { Id = id }).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Get Detail ExtraPractice Skill
        /// </summary>
        [HttpGet("skill/{id}")]
        [ProducesResponseType(typeof(MethodResult<ExtraPracticeSkillModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetSkill([FromRoute] Guid id)
        {
            MethodResult<ExtraPracticeSkillModel> commandResult = await _mediator.Send(new GetExtraPracticeSkillByIdQuery { Id = id }).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Get MockTests Not ExtraPratice
        /// </summary>
        [HttpGet("mock-tests")]
        [ProducesResponseType(typeof(MethodResult<IList<MockTestSearchModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetMockTests([FromQuery] EnumMockTestType? type)
        {
            MethodResult<IList<MockTestSearchModel>> commandResult = await _mediator.Send(new GetMockTestsByExtraPracticeQuery { Type = type }).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Delete a ExtraPractice
        /// </summary>
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(MethodResult<bool>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Delete([FromRoute] Guid id)
        {
            MethodResult<bool> commandResult = await _mediator.Send(new DeleteExtraPracticeCommand { Id = id }).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }
    }
}
