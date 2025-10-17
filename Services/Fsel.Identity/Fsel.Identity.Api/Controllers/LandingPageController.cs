// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Api.Controllers
{
    using System.Net;
    using Asp.Versioning;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Constants;
    using Fsel.Identity.Application.Commands.LandingPages;
    using Fsel.Shared.Constants;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;

    [ApiVersion(ApiSettings.APIVersion1)]
    [ApiVersion(ApiSettings.APIVersion1i1)]
    [Route(Settings.APIDefaultRoute + "/landing-page")]
    [ApiController]
    public class LandingPageController : ControllerBase
    {
        private readonly IMediator _mediator;

        public LandingPageController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Student Register Form To Google Sheet
        /// </summary>
        [HttpPost("student-register-form")]
        [ProducesResponseType(typeof(MethodResult<bool>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> LeadsIntegration([FromBody] StudentRegisterFormToGoogleSheetCommand query)
        {
            var commandResult = await _mediator.Send(query).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }
    }
}
