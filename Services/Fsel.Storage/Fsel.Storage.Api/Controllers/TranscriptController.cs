// Copyright (c) Atlantic. All rights reserved.

using System.Net;
using Fsel.Common.ActionResults;
using Fsel.Common.Constants;
using Fsel.Core.Base.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Fsel.Storage.Api.Controllers
{
    [ApiVersion(Settings.APIVersion)]
    [Route(Settings.APIDefaultRoute + "/transcript")]
    [ApiController]
    public class TranscriptController : ControllerBase
    {
        private readonly IDeepgramProvider _deepgramProvider;
        private readonly ICognitiveProvider _cognitiveProvider;

        public TranscriptController(IDeepgramProvider deepgramProvider, ICognitiveProvider cognitiveProvider)
        {
            _deepgramProvider = deepgramProvider;
            _cognitiveProvider = cognitiveProvider;
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

        /// <summary>
        /// Get Transcription
        /// </summary>
        [HttpPost("speech")]
        [ProducesResponseType(typeof(MethodResult<string>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> PostSpeech(string url)
        {
            MethodResult<string> result = new MethodResult<string>();
            result.Result = await _cognitiveProvider.GetTranscriptionAsync(url);
            return result.GetActionResult();
        }
    }
}
