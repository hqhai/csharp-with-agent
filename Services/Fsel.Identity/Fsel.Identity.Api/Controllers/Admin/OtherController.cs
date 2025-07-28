// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Api.Controllers.Admin
{
    using System.Net;
    using Asp.Versioning;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Attributes;
    using Fsel.Common.Constants;
    using Fsel.Identity.Application.Commands.AdminCmd;
    using Fsel.Identity.Application.Commands.OtherCmd;
    using Fsel.Identity.Application.Commands.UserCmd;
    using Fsel.Identity.Application.Queries.CompetitionEventsQuery;
    using Fsel.Identity.Domain.Models.EntityModels;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Models.ShareModels;
    using Fsel.Shared.Models.ShareModels.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;

    [ApiVersion(ApiSettings.APIVersion1)]
    [ApiVersion(ApiSettings.APIVersion1i1)]
    [Route(Settings.APIDefaultRoute + "/admin/other")]
    [ApiController]
    public class OtherController : ControllerBase
    {
        private readonly IMediator _mediator;

        public OtherController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// RetakeCourse
        /// </summary>
        [HttpGet("report-competition-event")]
        [ProducesResponseType(typeof(MethodResult<IList<ReportCompetitionEventModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [Permission(role: nameof(EnumRole.Admin))]
        public async Task<IActionResult> ReportCompetitionEvent([FromQuery] GetReportCompetitionEventsQuery query)
        {
            var commandResult = await _mediator.Send(query).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// RetakeCourse School
        /// </summary>
        [HttpGet("report-competition-event-school")]
        [ProducesResponseType(typeof(MethodResult<IList<ReportCompetitionEventModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [Permission(role: nameof(EnumRole.Admin))]
        public async Task<IActionResult> ReportCompetitionEvent([FromQuery] GetReportCompetitionEventSchoolsQuery query)
        {
            var commandResult = await _mediator.Send(query).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// RetakeCourse School
        /// </summary>
        [HttpGet("report-competition-event-district-school")]
        [ProducesResponseType(typeof(MethodResult<IList<ReportCompetitionEventModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [Permission(role: nameof(EnumRole.Admin))]
        public async Task<IActionResult> ReportCompetitionEvent([FromQuery] GetCompetitionEventToEventParentQuery query)
        {
            var commandResult = await _mediator.Send(query).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Get Student Event Registration
        /// </summary>
        [HttpGet("get-student-event-registrations")]
        [ProducesResponseType(typeof(MethodResult<IList<EventRegistrationModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [Permission(role: nameof(EnumRole.Admin))]
        public async Task<IActionResult> Get([FromQuery] GetStudentEventRegistrationsQuery query)
        {
            var commandResult = await _mediator.Send(query).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Add Coin By Sheet
        /// </summary>
        [HttpPost("add-coin-sheet")]
        [ProducesResponseType(typeof(MethodResult<bool>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [Permission(role: nameof(EnumRole.Admin))]
        public async Task<IActionResult> AddCoinBySheet([FromBody] AddCoinBySheetCommand query)
        {
            var commandResult = await _mediator.Send(query).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Import Users To Blind Bag Event
        /// </summary>
        [HttpPost("import-users-to-blind-bag-event")]
        [ProducesResponseType(typeof(MethodResult<CreateStudentsToEventFromFileModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [Permission(AddStudentToBlindBox.Add)]
        public async Task<IActionResult> ImportUsers([FromQuery] ImportUsersToBlindBagEventCommand command)
        {
            var commandResult = await _mediator.Send(command).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Add Coin By Sheet
        /// </summary>
        [HttpPost("add-coin-buy-course")]
        [ProducesResponseType(typeof(MethodResult<bool>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> AddCoinBuyCourseBySheet([FromBody] AddCoinBuyCourseBySheetCommand query)
        {
            var commandResult = await _mediator.Send(query).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Add Coin By Sheet
        /// </summary>
        [HttpPost("add-coin-fsel-event-reward")]
        [ProducesResponseType(typeof(MethodResult<bool>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> AddCoinFselEventRewardBySheet()
        {
            var commandResult = await _mediator.Send(new AddCoinFselEventRewardBySheetCommand()).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Update or delete test data for a student
        /// </summary>
        [HttpPut("{studentId}/delete-test")]
        [ProducesResponseType(typeof(MethodResult<bool>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> UpdateOrDeleteTest(Guid studentId)
        {
            var commandResult = await _mediator.Send(new UpdateDeleteTestCommand { StudentId = studentId }).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }
    }
}
