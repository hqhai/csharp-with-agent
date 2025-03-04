// Copyright (c) Atlantic. All rights reserved.

using System.Net;
using Fsel.Common.ActionResults;
using Fsel.Common.Constants;
using Fsel.Shared.Attributes;
using Fsel.Shared.Constants;
using Fsel.Storage.Application.Command.SpeechToTextCmd.V1i2;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Fsel.Storage.Api.Controllers.V1i2
{
    [ApiVersions(ApiSettings.APIVersion1i2)]
    [Route(Settings.APIDefaultRoute + "/transcript")]
    [ApiController]
    public class TranscriptController : ControllerBase
    {
        private readonly IMediator _mediator;

        public TranscriptController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// convert speech to text
        /// </summary>
        [DisableFormValueModelBinding]
        [DisableRequestSizeLimit]
        [RequestFormLimits(ValueLengthLimit = int.MaxValue, MultipartBodyLengthLimit = long.MaxValue)]
        [HttpPost("speech-to-text")]
        [ProducesResponseType(typeof(MethodResult<string>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> ConvertSpeechToText([FromQuery] ConvertSpeechToTextCommand request)
        {
            var methodResult = await _mediator.Send(request);
            return methodResult.GetActionResult();
        }
    }
}
