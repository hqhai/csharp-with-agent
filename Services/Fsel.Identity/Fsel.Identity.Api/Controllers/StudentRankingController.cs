// Copyright (c) Atlantic. All rights reserved.

using System.Net;
using Asp.Versioning;
using Fsel.Common.ActionResults;
using Fsel.Common.Constants;
using Fsel.Core.Base.BaseModels;
using Fsel.Identity.Application.Commands.CompetitionEventsCmd;
using Fsel.Identity.Application.Commands.LandingPages;
using Fsel.Identity.Application.Commands.StudentRankingCmd;
using Fsel.Identity.Application.Commands.StudentRankingEvents;
using Fsel.Identity.Application.Queries.CompetitionEventsQuery;
using Fsel.Identity.Application.Queries.StudentRanking;
using Fsel.Identity.Domain.Models.EntityModels;
using Fsel.Shared.Constants;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Fsel.Identity.Api.Controllers
{
    [ApiVersion(ApiSettings.APIVersion1)]
    [ApiVersion(ApiSettings.APIVersion1i1)]
    [Route(Settings.APIDefaultRoute + "/student-ranking")]
    [ApiController]
    public class StudentRankingController : ControllerBase
    {
        private readonly IMediator _mediator;

        public StudentRankingController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Lưu xếp hạng
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(MethodResult<List<StudentRankingModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> CreateStudentRanking()
        {
            MethodResult<List<StudentRankingModel>> commandResult = await _mediator.Send(new CreateStudentRankingsCommand()).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Lấy ra list danh sách xếp hạng của học sinh
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        [HttpGet]
        [ProducesResponseType(typeof(MethodResult<List<StudentRankingModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetStudentRanking([FromQuery] GetStudentRankingQuery query)
        {
            MethodResult<List<StudentRankingModel>> queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Lấy ra list danh sách xếp hạng của học sinh
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        [HttpGet("student-competition-ranking")]
        [ProducesResponseType(typeof(MethodResult<PagingItemsModel<StudentRankingModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetStudentRanking([FromQuery] GetStudentRankingCompetitionQuery query)
        {
            MethodResult<PagingItemsModel<StudentRankingModel>> queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Lấy ra list danh sách xếp hạng của học sinh
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        [HttpGet("student-competition-school")]
        [ProducesResponseType(typeof(MethodResult<PagingItemStudentRankingModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetStudentRankingForSchool([FromQuery] GetStudentCompetitionSchoolQuery query)
        {
            ArgumentNullException.ThrowIfNull(query);
            MethodResult<PagingItemStudentRankingModel> queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Tạo dữ liệu sự kiện
        /// </summary>
        [HttpPost("competition-events")]
        [ProducesResponseType(typeof(MethodResult<CompetitionEventsModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> CreateCompetitionEvents([FromBody] CreateCompetitionEventsCommand cmd)
        {
            MethodResult<CompetitionEventsModel> commandResult = await _mediator.Send(cmd).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Lấy liệu sự kiện
        /// </summary>
        [HttpGet("competition-events/{eventCode}")]
        [ProducesResponseType(typeof(MethodResult<CompetitionEventsModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetCompetitionEvents([FromRoute] string? eventCode)
        {
            MethodResult<CompetitionEventsModel> commandResult = await _mediator.Send(new GetCompetitionEventsQuery { EventCode = eventCode }).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Lưu dữ liệu sự kiện
        /// </summary>
        [HttpPost("student-ranking-events")]
        [ProducesResponseType(typeof(MethodResult<IList<StudentRankingEventsModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> CreateStudentRankingEvents([FromBody] CreateStudentRankingEventsCommand cmd)
        {
            MethodResult<IList<StudentRankingEventsModel>> commandResult = await _mediator.Send(cmd).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Check dữ liệu học sinh có lucky spin không ?
        /// </summary>
        [HttpGet("check-lucky-spin")]
        [ProducesResponseType(typeof(MethodResult<bool>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> CheckLuckySpin([FromQuery] CheckStudentLuckySpinCmd query)
        {
            var commandResult = await _mediator.Send(query).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// get events by user id
        /// </summary>
        [HttpGet("get-events-by-user-id")]
        [ProducesResponseType(typeof(MethodResult<bool>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetEventsByUserId([FromQuery] GetEventsByUserIdQuery query)
        {
            var commandResult = await _mediator.Send(query).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Form Register Student For Event
        /// </summary>
        [HttpPost("form-register-student-for-event")]
        [ProducesResponseType(typeof(MethodResult<bool>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> FormRegisterStudentForEvent([FromBody] FormRegisterStudentForEventCommand command)
        {
            var commandResult = await _mediator.Send(command).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }
    }
}
