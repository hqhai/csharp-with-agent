// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Api.Controllers
{
    using System.Net;
    using Asp.Versioning;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Constants;
    using Fsel.Identity.Application.Commands.StudentCmd;
    using Fsel.Shared.Constants;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;

    [ApiVersion(ApiSettings.APIVersion1)]
    [ApiVersion(ApiSettings.APIVersion1i1)]
    [Route(Settings.APIDefaultRoute + "/public")]
    [ApiController]
    public class PublicController : ControllerBase
    {
        private readonly IMediator _mediator;

        public PublicController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// export template create students to event
        /// </summary>
        [HttpPost("export-template-create-students-to-event-from-file")]
        [ProducesResponseType(typeof(MethodResult<Stream>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> ExportTemplateStudentsIntoPlatform()
        {
            MethodResult<Stream> commandResult = await _mediator.Send(new ExportTemplateCreateStudentsToEventFromFileCommand { }).ConfigureAwait(false);
            if (!commandResult.IsOK || commandResult.Result == null)
            {
                return commandResult.GetActionResult();
            }
            return File(commandResult.Result, Settings.Excels.ContentType, "Template_Import_StudentHN.xlsx");
        }

        [RequestSizeLimit(1 * 1024 * 1024)] // 1 MB
        [RequestFormLimits(MultipartBodyLengthLimit = 1 * 1024 * 1024)]
        [HttpPost("create-students-to-event-from-file")]
        [ProducesResponseType(typeof(MethodResult<bool>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> CreateStudentsFromFile([FromForm] CreateStudentsFromFileCommand command)
        {
            var commandResult = await _mediator.Send(command).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// check trường đã import chưa
        /// </summary>
        [HttpPost("check-school-import")]
        [ProducesResponseType(typeof(MethodResult<bool>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> CheckSchoolImportHistory([FromQuery] CheckSchoolImportHistoryCommand comand)
        {
            MethodResult<bool> commandResult = await _mediator.Send(comand).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }
    }
}
