// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Api.Controllers
{
    using System.Net;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Constants;
    using Fsel.Interaction.Application.Commands.CustomerSurveyCmd;
    using Fsel.Interaction.Application.Queries.CustomerSurveyQuery;
    using Fsel.Interaction.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;

    [ApiVersion(Settings.APIVersion)]
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
        [Authorize]
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
        [HttpGet("Student/{id}")]
        [ProducesResponseType(typeof(MethodResult<bool>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> IsSurveyCompletedByIdStudent([FromRoute] Guid id)
        {
            MethodResult<bool> queryResult = await _mediator.Send(new GetStudentByIdQuery { Id = id }).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }
    }
}
