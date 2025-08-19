// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Api.Controllers
{
    using System.Net;
    using Asp.Versioning;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Constants;
    using Fsel.Identity.Application.Queries.IntegrationQuery;
    using Fsel.Identity.Domain.Models.EntityModels.IntegrationModel;
    using Fsel.Shared.Constants;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;

    [ApiVersion(ApiSettings.APIVersion1)]
    [ApiVersion(ApiSettings.APIVersion1i1)]
    [Route(Settings.APIDefaultRoute + "/integration")]
    [ApiController]
    public class IntegrationController : ControllerBase
    {
        private readonly IMediator _mediator;

        public IntegrationController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// lead
        /// </summary>
        [HttpGet("leads")]
        [ProducesResponseType(typeof(MethodResult<LeadsIntegrationModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> LeadsIntegration([FromQuery] LeadsIntegrationQuery query)
        {
            var commandResult = await _mediator.Send(query).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// client
        /// </summary>
        [HttpGet("clients")]
        [ProducesResponseType(typeof(MethodResult<ClientsIntegrationModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> ClientsIntegration([FromQuery] ClientIntegrationQuery query)
        {
            var commandResult = await _mediator.Send(query).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// client
        /// </summary>
        [HttpGet("clients-event")]
        [ProducesResponseType(typeof(MethodResult<ClientsIntegrationModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> ClientIntegrationByEvent([FromQuery] ClientIntegrationByEventQuery query)
        {
            var commandResult = await _mediator.Send(query).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// lead
        /// </summary>
        [HttpGet("leads-event")]
        [ProducesResponseType(typeof(MethodResult<LeadsIntegrationModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> LeadsIntegrationByEvent([FromQuery] LeadsIntegrationByEventQuery query)
        {
            var commandResult = await _mediator.Send(query).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }
    }
}
