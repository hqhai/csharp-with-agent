// Copyright (c) Atlantic. All rights reserved.

using System.Net;
using Asp.Versioning;
using Fsel.Common.ActionResults;
using Fsel.Common.Constants;
using Fsel.Core.Base;
using Fsel.Core.Base.BaseModels;
using Fsel.Identity.Application.Commands.CompetitionEventsCmd;
using Fsel.Identity.Application.Queries.CompetitionEventsQuery;
using Fsel.Identity.Application.Services.SystemService.Model;
using Fsel.Identity.Domain.IRepositories;
using Fsel.Identity.Domain.Models.EntityModels;
using Fsel.Shared.Constants;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Fsel.Identity.Api.Controllers
{
    [ApiVersion(ApiSettings.APIVersion1)]
    [ApiVersion(ApiSettings.APIVersion1i1)]
    [Route(Settings.APIDefaultRoute + "/event")]
    [ApiController]
    public class EventController : BaseController
    {
        private readonly IMediator _mediator;
        private readonly ICompetitionEventsRepository _competitionEventsRepository;

        public EventController(IMediator mediator, ICompetitionEventsRepository competitionEventsRepository)
        {
            _mediator = mediator;
            _competitionEventsRepository = competitionEventsRepository;
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
        /// Execute-list-query
        /// </summary>
        [HttpGet("execute-list-query")]
        [ProducesResponseType(typeof(MethodResult<IList<CompetitionEventsModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> ExecuteList([FromQuery] BaseQueryModel query)
        {
            var result = await _competitionEventsRepository.GetListResultAsync<CompetitionEventsModel>(query);
            return result.GetActionResult();
        }
    }
}
