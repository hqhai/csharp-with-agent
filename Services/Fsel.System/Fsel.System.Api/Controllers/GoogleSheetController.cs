// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Api.Controllers
{
    using Asp.Versioning;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Attributes;
    using Fsel.Common.Constants;
    using Fsel.Shared.Constants;
    using Fsel.System.Application.Commands.GoogleSheets;
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
        [ServerCache(CacheSettings.TimeCache.OneHour)]
        [HttpGet("file-i18n")]
        [ProducesResponseType(typeof(MethodResult<I18NModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetDataFromFileI18N()
        {
            var commandResult = await _mediator.Send(new GetDataFromFileI18NQuery()).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Get data from file i18n
        /// </summary>
        [HttpPost("add-payment-info-to-google-sheet")]
        [ProducesResponseType(typeof(MethodResult<VoidMethodResult>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> AddPaymentInfoToGoogleSheet([FromBody] AddPaymentInfoToGoogleSheetCommand command)
        {
            var commandResult = await _mediator.Send(command).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Add Contact Info To Google Sheet File
        /// </summary>
        [HttpPost("add-contact-info-to-google-sheet-file")]
        [ProducesResponseType(typeof(MethodResult<bool>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> AddData([FromBody] AddContactInfoToGoogleSheetFileCommand command)
        {
            var commandResult = await _mediator.Send(command).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Add Contact Info From Landing Page FSEL To GoogleSheet
        /// </summary>
        [HttpPost("add-contact-info-from-landing-page")]
        [ProducesResponseType(typeof(MethodResult<bool>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> AddContactInfoFromLPFSELToGoogleSheet([FromBody] AddContactInfoFromLPFSELToGoogleSheetCommand command)
        {
            var commandResult = await _mediator.Send(command).ConfigureAwait(false);
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

        /// <summary>
        /// Add vouchers for MA
        /// </summary>
        [HttpPost("add-vouchers-for-ma")]
        [ProducesResponseType(typeof(MethodResult<bool>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> AddVouchersForMA([FromBody] AddVouchersForMAIntoGoogleSheetCommand command)
        {
            var commandResult = await _mediator.Send(command).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Add Contact Info From Landing Page FSEL To GoogleSheet
        /// </summary>
        [HttpPost("register-student-for-event")]
        [ProducesResponseType(typeof(MethodResult<bool>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> RegisterStudentForEvent([FromBody] RegisterStudentForEventCommand command)
        {
            var commandResult = await _mediator.Send(command).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Add Error Report Explanation Question  To GoogleSheet
        /// </summary>
        [HttpPost("add-error-report-explanation-question")]
        [ProducesResponseType(typeof(MethodResult<bool>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> AddErrorReportExplanationQuestionToGoogleSheet([FromBody] AddErrorReportExplanationQuestionToGoogleSheetCommand command)
        {
            var commandResult = await _mediator.Send(command).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Add Dynamic Info To Google Sheet File
        /// </summary>
        [HttpPost("add-dynamic-info-to-google-sheet-file")]
        [ProducesResponseType(typeof(MethodResult<bool>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> AddDynamicInfo([FromBody] AddDynamicInfoToGoogleSheetFileCommand command)
        {
            var commandResult = await _mediator.Send(command).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }
    }
}
