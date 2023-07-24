// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Api.Controllers
{
    using Fsel.Common.ActionResults;
    using System.Net;
    using Fsel.Common.Constants;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Interaction.Application.Queries.SupportQuestionQuery;
    using Fsel.Interaction.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;
    using Fsel.Interaction.Application.Commands.SupportQuetionCmd;
    using Fsel.Interaction.Application.Queries.SupportTicketQuery;
    using Fsel.Interaction.Application.Commands.SupportTicketCmd;

    [ApiVersion(Settings.APIVersion)]
    [Route(Settings.APIDefaultRoute + "/support-ticket")]
    [ApiController]
    public class SupportTicketController : ControllerBase
    {
        private readonly IMediator _mediator;

        public SupportTicketController(IMediator mediator)
        {
            _mediator = mediator;
        }
        /// <summary>
        /// Search Support Ticket
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(MethodResult<PagingItemsModel<SupportTicketModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Search([FromQuery] SearchSupportTicketQuery query)
        {
            MethodResult<PagingItemsModel<SupportTicketModel>> queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }


        /// <summary>
        /// Update a Support Ticket
        /// </summary>
        [HttpPut("update-status/{id}")]
        [ProducesResponseType(typeof(MethodResult<SupportTicketModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> UpdateStatus([FromRoute] Guid id, [FromBody] UpdateStatusSupportTicketCommand command)
        {
            ArgumentNullException.ThrowIfNull(command);
            command.Id = id;
            MethodResult<SupportTicketModel> commandResult = await _mediator.Send(command).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }
        /// <summary>
        /// Get Support Ticket
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(MethodResult<SupportTicketModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Get([FromRoute] Guid id)
        {
            MethodResult<SupportTicketModel> commandResult = await _mediator.Send(new GetSupportTicketQuery { Id = id }).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }
    }
}
