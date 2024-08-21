// Copyright (c) Atlantic. All rights reserved.

using System.Net;
using Asp.Versioning;
using Fsel.Common.ActionResults;
using Fsel.Common.Constants;
using Fsel.Core.Base.BaseModels;
using Fsel.Identity.Application.Commands.LandingPages;
using Fsel.Identity.Application.Commands.StudentRankingCmd;
using Fsel.Identity.Application.Commands.StudentRankingEvents;
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
        /// Lấy ra list danh sách xếp hạng của học sinh từ Excel
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
        /// Lưu dữ liệu sự kiện
        /// </summary>
        [HttpPost("student-competition-event")]
        [ProducesResponseType(typeof(MethodResult<IList<StudentCompetitionEventsModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> CreateStudentRankingEvents([FromBody] CreateStudentCompetitionEventsCommand cmd)
        {
            MethodResult<IList<StudentCompetitionEventsModel>> commandResult = await _mediator.Send(cmd).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Lấy dữ liệu event dựa vào học sinh
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
        /// Check dữ liệu học sinh có trong sự kiện lucky spin không ?
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
        /// static-data : fsel-3208
        /// Lấy dữ liệu student-ranking dựa trên hệ thống fsel
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        [HttpGet("school-event")]
        [ProducesResponseType(typeof(MethodResult<PagingItemStudentRankingModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetLeaderBoardData([FromQuery] GetStudentCompetitionByEventCodeQuery query)
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

        /// <summary>
        ///  Register Student For Event
        /// </summary>
        [HttpPost("register-student-for-event")]
        [ProducesResponseType(typeof(MethodResult<bool>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> RegisterStudentForEvent([FromBody] RegisterStudentForEventCommand command)
        {
            var commandResult = await _mediator.Send(command).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }
    }
}
