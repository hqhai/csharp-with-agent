// Copyright (c) Atlantic. All rights reserved.

using System.Net;
using Fsel.Common.ActionResults;
using Fsel.Common.Constants;
using Fsel.Core.Base.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Asp.Versioning;
using Fsel.Shared.Constants;
using Fsel.Storage.Domain.Models.CommandModels;
using Fsel.Storage.Domain.Enums;
using Fsel.Storage.Application.Services.AmazonS3Services;
using Fsel.Storage.Domain.Models.EntityModels;

namespace Fsel.Storage.Api.Controllers
{
    [ApiVersion(ApiSettings.APIVersion1)]
    [ApiVersion(ApiSettings.APIVersion1i1)]
    [Route(Settings.APIDefaultRoute + "/transcript")]
    [ApiController]
    public class TranscriptController : ControllerBase
    {
        private readonly IDeepgramProvider _deepgramProvider;
        private readonly ICognitiveProvider _cognitiveProvider;
        private readonly IAmazonS3Service _amazonS3Service;

        public TranscriptController(IDeepgramProvider deepgramProvider, ICognitiveProvider cognitiveProvider, IAmazonS3Service amazonS3Service)
        {
            _deepgramProvider = deepgramProvider;
            _cognitiveProvider = cognitiveProvider;
            _amazonS3Service = amazonS3Service;
        }

        /// <summary>
        /// Get Transcription
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(MethodResult<string>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Post([FromBody] UrlRequestModel request)
        {
            MethodResult<string> result = new MethodResult<string>();
            result.Result = await _deepgramProvider.GetTranscriptionAsync(request?.Url ?? string.Empty);
            return result.GetActionResult();
        }

        /// <summary>
        /// Get Transcription
        /// </summary>
        [HttpPost("file")]
        [ProducesResponseType(typeof(MethodResult<TranscriptFileModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Post([FromRoute] EnumFolderType folderType, [FromQuery] EnumBucketType? bucketType, IFormFile file, [FromQuery] bool isResize = false, [FromQuery] bool isValidEmpty = false)
        {
            MethodResult<TranscriptFileModel> result = new MethodResult<TranscriptFileModel>();

            var uploadResult = await _amazonS3Service.UploadFileAsync(bucketType, file, folderType, isResize, isValidEmpty);
            if (!uploadResult.IsOK)
            {
                result.AddError(uploadResult.ErrorMessages);
                return result.GetActionResult();
            }

            result.Result = new TranscriptFileModel
            {
                FilePath = uploadResult.Result,
                Content = await _deepgramProvider.GetTranscriptionAsync(file)
            };
            return result.GetActionResult();
        }

        /// <summary>
        /// Get Transcription
        /// </summary>
        [HttpPost("speech")]
        [ProducesResponseType(typeof(MethodResult<string>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> PostSpeech([FromBody] UrlRequestModel request)
        {
            MethodResult<string> result = new MethodResult<string>();
            result.Result = await _cognitiveProvider.GetTranscriptionAsync(request?.Url ?? string.Empty);
            return result.GetActionResult();
        }
    }
}
