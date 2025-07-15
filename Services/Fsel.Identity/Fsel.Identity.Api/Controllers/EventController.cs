// Copyright (c) Atlantic. All rights reserved.

using System.Net;
using Asp.Versioning;
using AutoMapper;
using Fsel.Common.ActionResults;
using Fsel.Common.Attributes;
using Fsel.Common.Constants;
using Fsel.Core.Base;
using Fsel.Core.Base.BaseModels;
using Fsel.Identity.Application.Commands.CompetitionEventsCmd;
using Fsel.Identity.Application.Commands.StudentCmd.StudentEventCmd;
using Fsel.Identity.Application.Queries.CompetitionEventsQuery;
using Fsel.Identity.Application.Queries.EventQuery;
using Fsel.Identity.Application.Services.SystemService.Model;
using Fsel.Identity.Domain.Entities;
using Fsel.Identity.Domain.IRepositories;
using Fsel.Identity.Domain.Models;
using Fsel.Identity.Domain.Models.EntityModels;
using Fsel.Shared.Constants;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Fsel.Identity.Api.Controllers
{
    [ApiVersion(ApiSettings.APIVersion1)]
    [ApiVersion(ApiSettings.APIVersion1i1)]
    [Route(Settings.APIDefaultRoute + "/event")]
    [ApiController]
    [Permission]
    public class EventController : BaseController
    {
        private readonly IMediator _mediator;
        private readonly ICompetitionEventsRepository _competitionEventsRepository;
        private readonly IMapper _mapper;

        public EventController(IMediator mediator, ICompetitionEventsRepository competitionEventsRepository, IMapper mapper)
        {
            _mediator = mediator;
            _competitionEventsRepository = competitionEventsRepository;
            _mapper = mapper;
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
        public async Task<IActionResult> GetCompetitionEvents([FromRoute] string? eventCode, [FromQuery] bool isLeaderBoard)
        {
            MethodResult<CompetitionEventsModel> commandResult = await _mediator.Send(new GetCompetitionEventsQuery { EventCode = eventCode, IsLeaderBoard = isLeaderBoard }).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Lấy danh sách trường học theo mã sự kiện
        /// </summary>
        [HttpGet("schools")]
        [ProducesResponseType(typeof(MethodResult<IList<SchoolModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetSchoolsByEventCode([FromQuery] GetSchoolsByEventCodeQuery query)
        {
            ArgumentException.ThrowIfNullOrEmpty(nameof(query));
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

        /// <summary>
        /// Execute-list-query
        /// </summary>
        [HttpGet("execute-list-query")]
        [ProducesResponseType(typeof(MethodResult<IList<CompetitionEventsModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> ExecuteList([FromQuery] BaseQueryModel query)
        {
            var competitionEvents = await _competitionEventsRepository.Queryable.ToListAsync(CancellationToken.None);
            var competitionEventModels = _mapper.Map<IList<CompetitionEventsModel>>(competitionEvents);
            var methodResult = new MethodResult<IList<CompetitionEventsModel>>();
            methodResult.Result = competitionEventModels;
            return methodResult.GetActionResult();
        }

        /// <summary>
        /// Export-LandingPage-By-EventCode
        /// </summary>
        [HttpGet("export-landing-page")]
        [ProducesResponseType(typeof(MethodResult<Stream>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> ExportLandingPageByEventCode([FromQuery] ExportLandingPageByEventCodeQuery query)
        {
            MethodResult<Stream> commandResult = await _mediator.Send(query).ConfigureAwait(false);
            if (!commandResult.IsOK || commandResult.Result == null)
            {
                return commandResult.GetActionResult();
            }
            return File(commandResult.Result, Settings.Excels.ContentType, $"LandingPageEventCode.xlsx");
        }

        /// <summary>
        /// Lấy danh sách Events theo ParentIds
        /// </summary>
        [HttpGet("get-events-by-parent-ids")]
        [ProducesResponseType(typeof(MethodResult<IList<CompetitionEventsModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetEventsByParentIds([FromQuery] GetCompetitionEventsByParentIdsQuery query)
        {
            ArgumentException.ThrowIfNullOrEmpty(nameof(query));
            var commandResult = await _mediator.Send(query).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        [HttpGet("get-event-parent/{id}")]
        [ProducesResponseType(typeof(MethodResult<CompetitionEventsModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetParentEvent([FromRoute] Guid id)
        {
            ArgumentException.ThrowIfNullOrEmpty(nameof(id));
            var commandResult = await _mediator.Send(new GetParentEventByIdQuery { Id = id }).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        [HttpGet("get-child-events/{id}")]
        [ProducesResponseType(typeof(MethodResult<IList<CompetitionEvent>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetChildPEvents([FromRoute] Guid id)
        {
            ArgumentException.ThrowIfNullOrEmpty(nameof(id));
            var commandResult = await _mediator.Send(new GetChildEventsByParentIdQuery { Id = id }).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Get event by ids
        /// </summary>
        [HttpPost("event-ids")]
        [ProducesResponseType(typeof(MethodResult<IList<CompetitionEventsModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetEventByIds([FromBody] GetCompetitionEventByIdsQuery query)
        {
            MethodResult<IList<CompetitionEventsModel>> queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Lấy danh sách Events theo EventCodeStr
        /// </summary>
        [HttpGet("get-events-by-event-code-str")]
        [ProducesResponseType(typeof(MethodResult<IList<CompetitionEventsModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetEventsByEventCodeStr([FromQuery] GetCompetitionEventsToEventCodeStrQuery query)
        {
            ArgumentException.ThrowIfNullOrEmpty(nameof(query));
            var commandResult = await _mediator.Send(query).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// aggregate data students in event
        /// </summary>
        [HttpPost("aggregate-data-students-in-event")]
        [ProducesResponseType(typeof(MethodResult<IList<CompetitionEventsModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> AggregateDataStudentsInEvent()
        {
            var commandResult = await _mediator.Send(new AggregateDataStudentsInEventCommand()).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// get student event learning record
        /// </summary>
        [HttpGet("get-student-event-learning-record")]
        [ProducesResponseType(typeof(MethodResult<StudentEventLearningRecordModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetStudentEventLearningRecord([FromQuery] GetStudentEventLearningRecordQuery query)
        {
            ArgumentException.ThrowIfNullOrEmpty(nameof(query));
            var commandResult = await _mediator.Send(query).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// update student event learning record
        /// </summary>
        [HttpPost("student-event-view-learning-record")]
        [ProducesResponseType(typeof(MethodResult<bool>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> UpdateStudentEventLearningRecord([FromBody] StudentEventViewLearningRecordCommand command)
        {
            var commandResult = await _mediator.Send(command).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        [HttpGet("get-tree-events")]
        [ProducesResponseType(typeof(MethodResult<IList<CompetitionEvent>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetTreeCompetitionEvent([FromQuery] GetTreeCompetitionEventQuery query)
        {
            var queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// get studentIds in event by studentIds
        /// </summary>
        [HttpPost("get-student-ids-in-event-by-student-ids")]
        [ProducesResponseType(typeof(MethodResult<List<Guid>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetStudentsInEventByStudentIds([FromBody] GetStudentIdsInEventByStudentIdsQuery query)
        {
            var commandResult = await _mediator.Send(query).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }
    }
}