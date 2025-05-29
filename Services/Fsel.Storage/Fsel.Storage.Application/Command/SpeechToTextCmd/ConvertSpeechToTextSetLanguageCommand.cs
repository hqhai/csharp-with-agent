// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Storage.Application.Command.SpeechToTextCmd
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Storage.Application.Services.OpenAIServices;
    using Fsel.Storage.Domain.Models.EntityModels;
    using Fsel.Storage.Infrastructure.ValueSettings;
    using MediatR;
    using Newtonsoft.Json;
    using Refit;

    public class ConvertSpeechToTextSetLanguageCommand : IRequest<MethodResult<string>>
    {
        public string? FilePath { get; set; }
    }

    public class ConvertSpeechToTextSetLanguageCommandHandler : IRequestHandler<ConvertSpeechToTextSetLanguageCommand, MethodResult<string>>
    {
        private readonly IOpenAIService _openAIService;
        private readonly AppSetting _appSetting;

        public ConvertSpeechToTextSetLanguageCommandHandler(IOpenAIService openAIService,
                                                            AppSetting appSetting)
        {
            _openAIService = openAIService;
            _appSetting = appSetting;
        }

        public async Task<MethodResult<string>> Handle(ConvertSpeechToTextSetLanguageCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request.FilePath);
            MethodResult<string> methodResult = new MethodResult<string>();

            using var httpClient = new HttpClient();
            var response = await httpClient.GetAsync(request.FilePath, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return methodResult;
            }
            var fileName = response.Content.Headers.ContentDisposition?.FileName?.Trim('"') ?? "audio.wav";
            var contentType = response.Content.Headers.ContentType?.ToString() ?? "audio/wav";
            var bytes = await response.Content.ReadAsByteArrayAsync(cancellationToken);

            var stream = new MemoryStream(bytes);
            StreamPart streamPart = new StreamPart(stream, fileName, contentType);

            var speechToText = await _openAIService.SpeechToTextByAISetLanguageAsync(streamPart, _appSetting.OpenAiConfig?.ApprovalAIModel, _appSetting.OpenAiConfig?.Language);
            if (!speechToText.IsSuccessStatusCode)
            {
                methodResult.AddError(speechToText.Error);
                return methodResult;
            }

            var convertContent = !string.IsNullOrEmpty(speechToText.Content) ? JsonConvert.DeserializeObject<ContentModel>(speechToText.Content)?.Text : string.Empty;
            methodResult.Result = convertContent;
            return methodResult;
        }
    }
}
