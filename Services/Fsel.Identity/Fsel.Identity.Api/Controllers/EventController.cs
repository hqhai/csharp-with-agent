// Copyright (c) Atlantic. All rights reserved.

using System.Net;
using Fsel.Common.ActionResults;
using Fsel.Common.Constants;
using Fsel.Identity.Domain.Models.EntityModels;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Asp.Versioning;
using Fsel.Shared.Constants;
using Fsel.Identity.Application.Commands.CompetitionEventsCmd;
using Fsel.Identity.Application.Queries.CompetitionEventsQuery;
using Fsel.Identity.Application.Services.SystemService.Model;

namespace Fsel.Identity.Api.Controllers
{
    [ApiVersion(ApiSettings.APIVersion1)]
    [ApiVersion(ApiSettings.APIVersion1i1)]
    [Route(Settings.APIDefaultRoute + "/event")]
    [ApiController]
    public class EventController : ControllerBase
    {
        private readonly IMediator _mediator;

        public EventController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Tạo dữ liệu sự kiện
        /// </summary>
        [HttpPost()]
        [ProducesResponseType(typeof(MethodResult<CompetitionEventsModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> CreateCompetitionEvents([FromBody] CreateCompetitionEventsCommand cmd)
        {
            MethodResult<CompetitionEventsModel> commandResult = await _mediator.Send(cmd).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// cập nhật dữ liệu sự kiện
        /// </summary>
        [HttpPut()]
        [ProducesResponseType(typeof(MethodResult<CompetitionEventsModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> UpdateCompetitionEvents([FromBody] UpdateCompetitionEventsCommand cmd)
        {
            MethodResult<CompetitionEventsModel> commandResult = await _mediator.Send(cmd).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Lấy liệu sự kiện dựa trên mã sự kiện
        /// </summary>
        [HttpGet("{eventCode}")]
        [ProducesResponseType(typeof(MethodResult<CompetitionEventsModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetCompetitionEvents([FromRoute] string? eventCode)
        {
            MethodResult<CompetitionEventsModel> commandResult = await _mediator.Send(new GetCompetitionEventsQuery { EventCode = eventCode }).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Lấy danh sách trường học theo mã sự kiện
        /// </summary>
        [HttpGet("schools/{eventCode}")]
        [ProducesResponseType(typeof(MethodResult<IList<SchoolModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetSchoolsByEventCode([FromRoute] string? eventCode)
        {
            ArgumentException.ThrowIfNullOrEmpty(nameof(eventCode));
            GetSchoolsByEventCodeQuery query = new GetSchoolsByEventCodeQuery();
            query.EventCode = eventCode;
            MethodResult<IList<SchoolModel>> commandResult = await _mediator.Send(query).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Lấy liệu sự kiện trong khoảng ngày
        /// </summary>
        [HttpGet("event-by-date")]
        [ProducesResponseType(typeof(MethodResult<IList<CompetitionEventsModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetCompetitionEventsByDate([FromQuery] GetCompetitionEventsByDateQuery query)
        {
            MethodResult<IList<CompetitionEventsModel>> commandResult = await _mediator.Send(query).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }
    }
}
