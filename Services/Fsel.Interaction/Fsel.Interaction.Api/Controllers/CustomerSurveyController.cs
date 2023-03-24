// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Api.Controllers
{
    using System.Net;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Constants;
    using Fsel.Interaction.Application.Commands.CustomerSurveyCmd;
    using Fsel.Interaction.Application.Queries;
    using Fsel.Interaction.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;

    [ApiVersion(Settings.APIVersion)]
    [Route(Settings.APIDefaultRoute + "/customerSurvey")]
    [ApiController]
    [Authorize]
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
        /// GetAll Survey Question
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(MethodResult<IList<SurveyQuestionModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetAll()
        {
            MethodResult<IList<SurveyQuestionModel>> queryResult = await _mediator.Send(new GetAllSurveyQuestQuery()).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }
    }
}
