// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Api.Controllers
{
    using System.Net;
    using Asp.Versioning;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Constants;
    using Fsel.Interaction.Application.Commands.CustomerSurveyCmd;
    using Fsel.Interaction.Application.Commands.SurveyConfigCmd;
    using Fsel.Interaction.Application.Queries.CustomerSurveyQuery;
    using Fsel.Interaction.Application.Queries.SurveyConfigQuery;
    using Fsel.Interaction.Domain.Models.EntityModels;
    using Fsel.Shared.Constants;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;

    [ApiVersion(ApiSettings.APIVersion1)]
    [ApiVersion(ApiSettings.APIVersion1i1)]
    [Route(Settings.APIDefaultRoute + "/customerSurvey")]
    [ApiController]
    public class CustomerSurveyController : ControllerBase
    {
        private readonly IMediator _mediator;

        public CustomerSurveyController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Create a Customer Survey
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(MethodResult<IList<CustomerSurveyModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Create([FromBody] CreateCustomerSurveyCommand command)
        {
            MethodResult<IList<CustomerSurveyModel>> queryResult = await _mediator.Send(command).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Check Student by id
        /// </summary>
        [HttpGet("IsCompleted/{id}")]
        [ProducesResponseType(typeof(MethodResult<bool>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> IsSurveyCompletedByStudentId([FromRoute] Guid id)
        {
            MethodResult<bool> queryResult = await _mediator.Send(new GetIsSurveyByStudentIdQuery { Id = id }).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Create a Customer Survey
        /// </summary>
        [HttpPost("customer-type")]
        [ProducesResponseType(typeof(MethodResult<bool>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> CreateCustomerByType([FromBody] CreateCustomerSurveyTypeEventCommand command)
        {
            var queryResult = await _mediator.Send(command).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Check survey pt
        /// </summary>
        [HttpGet("check-survey-pt")]
        [ProducesResponseType(typeof(MethodResult<bool>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> CheckSurveyBySurveyFormType([FromQuery] CheckSurveyPTQuery query)
        {
            MethodResult<bool> queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Save User Survey Assignment
        /// </summary>
        [HttpPost("save-user-survey-assignment")]
        [ProducesResponseType(typeof(MethodResult<bool>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> SaveUserSurveyAssignment([FromBody] SaveUserSurveyAssignmentCommand command)
        {
            var queryResult = await _mediator.Send(command).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Do survey
        /// </summary>
        [HttpPost("do-survey")]
        [ProducesResponseType(typeof(MethodResult<bool>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> DoSurvey([FromBody] StudentDoSurveyCommand command)
        {
            var queryResult = await _mediator.Send(command).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Do survey
        /// </summary>
        [HttpPost("change-status-view-survey")]
        [ProducesResponseType(typeof(MethodResult<bool>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> ChangeStatusViewSurvey([FromBody] ChangeStatusViewSurveyCommand command)
        {
            var queryResult = await _mediator.Send(command).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// get user surveys assignment
        /// </summary>
        [HttpGet("get-user-surveys-assignment")]
        [ProducesResponseType(typeof(MethodResult<IList<UserSurveyAssignmentModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetUserSurveysAssignment()
        {
            var queryResult = await _mediator.Send(new SearchUserSurveyAssignmentQuery()).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// get history do survey by user id
        /// </summary>
        [HttpGet("get-history-do-survey-by-user-id")]
        [ProducesResponseType(typeof(MethodResult<SurveyConfigModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetHistoryDoSurveyByUserId([FromQuery] GetHistoryDoSurveyByUserIdQuery query)
        {
            var queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }
    }
}
