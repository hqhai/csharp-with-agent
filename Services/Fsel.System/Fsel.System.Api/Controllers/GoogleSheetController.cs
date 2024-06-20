// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Api.Controllers
{
    using Asp.Versioning;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Constants;
    using Fsel.Shared.Constants;
    using Fsel.System.Application.Queries.GoogleSheets;
    using Fsel.System.Application.Services.GoogleSheetServices.Models;
    using global::System.Net;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;

    [ApiVersion(ApiSettings.APIVersion1)]
    [ApiVersion(ApiSettings.APIVersion1i1)]
    [Route(Settings.APIDefaultRoute + "/google-sheet")]
    [ApiController]
    public class GoogleSheetController : ControllerBase
    {
        private readonly IMediator _mediator;

        public GoogleSheetController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Get data from file i18n
        /// </summary>
        [HttpGet("file-i18n")]
        [ProducesResponseType(typeof(MethodResult<I18NModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetDataFromFileI18N()
        {
            var commandResult = await _mediator.Send(new GetDataFromFileI18NQuery()).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Get CC emails
        /// </summary>
        [HttpGet("get-cc-email")]
        [ProducesResponseType(typeof(MethodResult<bool>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetCCEmail()
        {
            var commandResult = await _mediator.Send(new GetCCEmailsAccordingToStudentsQuery()).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }
    }
}
