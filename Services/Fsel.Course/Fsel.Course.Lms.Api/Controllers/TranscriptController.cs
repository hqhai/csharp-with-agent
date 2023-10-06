// Copyright (c) Atlantic. All rights reserved.

using System.Net;
using Fsel.Common.ActionResults;
using Fsel.Common.Constants;
using Fsel.Core.Base.Interfaces;
using Fsel.Course.Lms.Application.Commands.VideoTimeCodeAnswerCmd;
using Fsel.Shared.Constants;
using Fsel.Shared.Models.ShareModels;
using Microsoft.AspNetCore.Mvc;

namespace Fsel.Course.Lms.Api.Controllers
{
    [ApiVersion(Settings.APIVersion)]
    [Route(Settings.APIDefaultRoute + "/transcript")]
    [ApiController]
    public class TranscriptController : ControllerBase
    {
        private readonly IDeepgramProvider _deepgramProvider;

        public TranscriptController(IDeepgramProvider deepgramProvider)
        {
            _deepgramProvider = deepgramProvider;
        }

        /// <summary>
        /// Get Transcription
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(MethodResult<string>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Post(string url)
        {
            MethodResult<string> result = new MethodResult<string>();
            result.Result = await _deepgramProvider.GetTranscriptionAsync(url);
            return result.GetActionResult();
        }
    }
}
