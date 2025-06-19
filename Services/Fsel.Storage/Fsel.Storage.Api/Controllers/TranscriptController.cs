// Copyright (c) Atlantic. All rights reserved.

using System.Net;
using Asp.Versioning;
using Fsel.Common.ActionResults;
using Fsel.Common.Constants;
using Fsel.Core.Base.Interfaces;
using Fsel.Shared.Attributes;
using Fsel.Shared.Constants;
using Fsel.Shared.Enums;
using Fsel.Storage.Application.Command.ChatbotCmd;
using Fsel.Storage.Application.Command.SpeechToTextCmd;
using Fsel.Storage.Application.Services.AmazonS3Services;
using Fsel.Storage.Domain.Models.CommandModels;
using Fsel.Storage.Domain.Models.EntityModels;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Fsel.Storage.Api.Controllers
{
    [ApiVersions(ApiSettings.APIVersion1)]
    [Route(Settings.APIDefaultRoute + "/transcript")]
    [ApiController]
    public class TranscriptController : ControllerBase
    {
        private readonly IDeepgramProvider _deepgramProvider;
        private readonly ICognitiveProvider _cognitiveProvider;
        private readonly IAmazonS3Service _amazonS3Service;
        private readonly IMediator _mediator;

        public TranscriptController(IDeepgramProvider deepgramProvider, ICognitiveProvider cognitiveProvider, IAmazonS3Service amazonS3Service, IMediator mediator)
        {
            _deepgramProvider = deepgramProvider;
            _cognitiveProvider = cognitiveProvider;
            _amazonS3Service = amazonS3Service;
            _mediator = mediator;
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
        [DisableFormValueModelBinding]
        [DisableRequestSizeLimit]
        [RequestFormLimits(ValueLengthLimit = int.MaxValue, MultipartBodyLengthLimit = long.MaxValue)]
        [ProducesResponseType(typeof(MethodResult<TranscriptFileModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [HttpPost("upload-file/{folderType}")]
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
        /// Get Chatbot-Speech
        /// </summary>
        [HttpPost("text-to-speech")]
        [ProducesResponseType(typeof(MethodResult<string>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> PostSpeech([FromBody] CreateChatbotAudioCommand cmd)
        {
            MethodResult<string> queryResult = await _mediator.Send(cmd).ConfigureAwait(false);
            return queryResult.GetActionResult();
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
            result.Result = await _deepgramProvider.GetTranscriptionAsync(request?.Url ?? string.Empty, "nova-2");
            return result.GetActionResult();
        }

        /// <summary>
        /// convert speech to text
        /// </summary>
        [MapToApiVersion(ApiSettings.APIVersion1)]
        [MapToApiVersion(ApiSettings.APIVersion1i1)]
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

        /// <summary>
        /// convert wav
        /// </summary>
        [DisableFormValueModelBinding]
        [DisableRequestSizeLimit]
        [RequestFormLimits(ValueLengthLimit = int.MaxValue, MultipartBodyLengthLimit = long.MaxValue)]
        [ProducesResponseType(typeof(MethodResult<string>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [HttpPost("convert-wav")]
        public async Task<IActionResult> ConvertFileToWav(IFormFile file)
        {
            var methodResult = await _mediator.Send(new ConvertFileToWAVCommand { FormFile = file });
            return methodResult.GetActionResult();
        }

        /// <summary>
        /// speech to text
        /// </summary>
        [HttpPost("convert-speech-to-text")]
        [ProducesResponseType(typeof(MethodResult<string>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> ToolConvertSpeechToText([FromBody] SpeechToTextCommand command)
        {
            var methodResult = await _mediator.Send(command);
            return methodResult.GetActionResult();
        }

        /// <summary>
        /// Speech To Text Set Language
        /// </summary>
        [HttpPost("speech-to-text-language")]
        [ProducesResponseType(typeof(MethodResult<string>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> ConvertSpeechToTextSetLanguage([FromBody] ConvertSpeechToTextSetLanguageCommand request)
        {
            var methodResult = await _mediator.Send(request);
            return methodResult.GetActionResult();
        }
    }
}
