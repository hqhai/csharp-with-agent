// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Storage.Application.Command.SpeechToTextCmd
{
    using System;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base.Interfaces;
    using Fsel.Storage.Application.Services.OpenAIServices;
    using Fsel.Storage.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Newtonsoft.Json;
    using Refit;

    public class SpeechToTextCommand : IRequest<MethodResult<string>>
    {
        public string FileName { get; set; } = string.Empty;

        public string ContentType { get; set; } = string.Empty;

        public byte[] FileData { get; set; } = Array.Empty<byte>();
    }

    public class SpeechToTextCommandHandler : IRequestHandler<SpeechToTextCommand, MethodResult<string>>
    {
        private readonly IOpenAIService _openAIService;
        private readonly ICognitiveProvider _cognitiveProvider;
        private const string AIModel = "whisper-1";

        public SpeechToTextCommandHandler(IOpenAIService openAIService,
                                          ICognitiveProvider cognitiveProvider)
        {
            _openAIService = openAIService;
            _cognitiveProvider = cognitiveProvider;
        }

        public async Task<MethodResult<string>> Handle(SpeechToTextCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<string> methodResult = new MethodResult<string>();
            IFormFile formFile = ConvertToIFormFile(request.FileData, request.FileName, request.ContentType);
            if (formFile == null)
            {
                return methodResult;
            }

            var stream = formFile.OpenReadStream();
            var streamPart = new StreamPart(stream, formFile.FileName, formFile.ContentType);

            var content = await _openAIService.SpeechToTextByAIAsync(streamPart, AIModel);

            if (content.Content == null || !content.IsSuccessStatusCode)
            {
                var contentDeepgram = await _cognitiveProvider.GetTranscriptionAsync(formFile, cancellationToken);
                methodResult.Result = contentDeepgram;
                return methodResult;
            }

            var convertContent = !string.IsNullOrEmpty(content.Content) ? JsonConvert.DeserializeObject<ContentModel>(content.Content)?.Text : string.Empty;
            methodResult.Result = convertContent;
            return methodResult;
        }

        private static IFormFile ConvertToIFormFile(byte[] fileData, string fileName, string contentType)
        {
            var stream = new MemoryStream(fileData);
            return new FormFile(stream, 0, fileData.Length, "file", fileName)
            {
                Headers = new HeaderDictionary(),
                ContentType = contentType
            };
        }
    }
}
