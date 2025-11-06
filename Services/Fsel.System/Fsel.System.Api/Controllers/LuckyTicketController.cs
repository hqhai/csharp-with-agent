// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Api.Controllers
{
    using Asp.Versioning;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Constants;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Enums;
    using Fsel.System.Application.Commands.GoogleSheets;
    using Fsel.System.Application.Commands.LuckyTickets;
    using Fsel.System.Application.Queries.LuckyTickets;
    using Fsel.System.Domain.Models.EntityModels;
    using global::System.Net;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;
    using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

    [ApiVersion(ApiSettings.APIVersion1)]
    [ApiVersion(ApiSettings.APIVersion1i1)]
    [Route(Settings.APIDefaultRoute + "/lucky-ticket")]
    [ApiController]
    public class LuckyTicketController : ControllerBase
    {
        private readonly IMediator _mediator;

        public LuckyTicketController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// create lucky ticket
        /// </summary>
        [HttpPost("create-lucky-ticket")]
        [ProducesResponseType(typeof(MethodResult<VoidMethodResult>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
       [Common.Attributes.Permission(roles: new string[] { nameof(EnumRole.Student), nameof(EnumRole.StudentCampus) })]
        public async Task<IActionResult> Create([FromBody] CreateLuckyTicketCommand command)
        {
            var commandResult = await _mediator.Send(command).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// update tickets winning
        /// </summary>
        [HttpPut("update-tickets-winning")]
        [ProducesResponseType(typeof(MethodResult<VoidMethodResult>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [Common.Attributes.Permission(role: nameof(EnumRole.Admin))]
        public async Task<IActionResult> Update([FromBody] UpdateLuckyTicketWinningCommand command)
        {
            var commandResult = await _mediator.Send(command).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// get all lucky ticket
        /// </summary>
        [HttpGet("get-all-lucky-ticket")]
        [ProducesResponseType(typeof(MethodResult<PagingItemsModel<StudentLuckyTicketModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetAll([FromQuery] GetAllLuckyTicketQuery query)
        {
            var commandResult = await _mediator.Send(query).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// get tickets winning
        /// </summary>
        [HttpGet("get-tickets-winning")]
        [ProducesResponseType(typeof(MethodResult<PagingItemsModel<StudentLuckyTicketModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetTicketsWinning([FromQuery] GetLuckyTicketsWinningQuery query)
        {
            var commandResult = await _mediator.Send(query).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// get tickets by student
        /// </summary>
        [HttpGet("get-tickets-by-student")]
        [ProducesResponseType(typeof(MethodResult<IList<string>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
       [Common.Attributes.Permission(roles: new string[] { nameof(EnumRole.Student), nameof(EnumRole.StudentCampus) })]
        public async Task<IActionResult> GetTicketsByStudent()
        {
            var commandResult = await _mediator.Send(new GetLuckyTicketsByStudentQuery()).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// get tickets by student
        /// </summary>
        [HttpGet("export-tickets")]
        [ProducesResponseType(typeof(MethodResult<IList<string>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> ExportTickets([FromQuery] AddLuckyTicketsToGoogleSheetCommand command)
        {
            var commandResult = await _mediator.Send(command).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }
    }
}
