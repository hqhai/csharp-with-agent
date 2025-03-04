// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Storage.Application.Command.SpeechToTextCmd
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base.Interfaces;
    using Fsel.Shared.Enums;
    using Fsel.Storage.Application.Services.AmazonS3Services;
    using Fsel.Storage.Application.Services.OpenAIServices;
    using Fsel.Storage.Domain.Enums.ErrorCodes;
    using Fsel.Storage.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Newtonsoft.Json;
    using Refit;

    public class ConvertSpeechToTextCommand : IRequest<MethodResult<TranscriptFileModel>>
    {
        public IFormFile? FormFile { get; set; }
    }

    public class ConvertSpeechToTextCommandHandler : IRequestHandler<ConvertSpeechToTextCommand, MethodResult<TranscriptFileModel>>
    {
        private readonly IOpenAIService _openAIService;
        private readonly IAmazonS3Service _amazonS3Service;
        private const string AIModel = "whisper-1";
        private readonly ICognitiveProvider _cognitiveProvider;


        public ConvertSpeechToTextCommandHandler(IOpenAIService openAIService, IAmazonS3Service amazonS3Service, ICognitiveProvider cognitiveProvider)
        {
            _openAIService = openAIService;
            _amazonS3Service = amazonS3Service;
            _cognitiveProvider = cognitiveProvider;
        }

        public async Task<MethodResult<TranscriptFileModel>> Handle(ConvertSpeechToTextCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<TranscriptFileModel> methodResult = new MethodResult<TranscriptFileModel>();

            if (request.FormFile == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumFileErrorCode.FormFileNotNull), nameof(request.FormFile), nameof(request.FormFile));
                return methodResult;
            }

            var stream = request.FormFile.OpenReadStream();
            var streamPart = new StreamPart(stream, request.FormFile.FileName, request.FormFile.ContentType);

            var content = await _openAIService.SpeechToTextByAIAsync(streamPart, AIModel);

            if (content.Content == null || !content.IsSuccessStatusCode)
            {
                var contentDeepgram = await _cognitiveProvider.GetTranscriptionAsync(request.FormFile, cancellationToken);
                var fileInfomationDeepGram = await UpLoadFileAsync(request.FormFile);
                methodResult.Result = new TranscriptFileModel { FilePath = fileInfomationDeepGram.Result, Content = contentDeepgram ?? string.Empty };
                return methodResult;
            }

            var fileInfomation = await UpLoadFileAsync(request.FormFile);
            var convertContent = !string.IsNullOrEmpty(content.Content) ? JsonConvert.DeserializeObject<ContentModel>(content.Content)?.Text : string.Empty;
            methodResult.Result = new TranscriptFileModel { FilePath = fileInfomation.Result, Content = convertContent };
            return methodResult;
        }

        public async Task<MethodResult<string?>> UpLoadFileAsync(IFormFile formFile)
        {
            return await _amazonS3Service.UploadFileAsync(EnumBucketType.FselPublic, formFile, EnumFolderType.Videos, false, false);
        }
    }
}
