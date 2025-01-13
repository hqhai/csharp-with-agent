// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Storage.Application.Command.SpeechToTextCmd
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Storage.Application.Services.OpenAIServices;
    using Fsel.Storage.Domain.Enums.ErrorCodes;
    using MediatR;
    using Refit;

    public class ConvertSpeechToTextCommand : IRequest<MethodResult<string>>
    {
        public string? Url { get; set; }
    }

    public class ConvertSpeechToTextCommandHandler : IRequestHandler<ConvertSpeechToTextCommand, MethodResult<string>>
    {
        private readonly IOpenAIService _openAIService;
        private const string Model = "whisper-1";

        public ConvertSpeechToTextCommandHandler(IOpenAIService openAIService)
        {
            _openAIService = openAIService;
        }

        public async Task<MethodResult<string>> Handle(ConvertSpeechToTextCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<string> methodResult = new MethodResult<string>();

            if (string.IsNullOrEmpty(request.Url))
            {
                methodResult.AddErrorBadRequest(nameof(EnumFileErrorCode.UrlNotNull), nameof(request.Url), nameof(request.Url));
                return methodResult;
            }

            var content = await GetTextFromUrl(request.Url);
            if (!content.IsOK)
            {
                methodResult.AddErrorBadRequest(content.ErrorMessages);
                return methodResult;
            }

            methodResult.Result = content.Result;
            return methodResult;
        }

        private async Task<MethodResult<string>> GetTextFromUrl(string url)
        {
            MethodResult<string> methodResult = new MethodResult<string>();
            using (HttpClient client = new HttpClient())
            {
                // Gửi yêu cầu GET để tải file từ URL
                using (HttpResponseMessage response = await client.GetAsync(url))
                {
                    string contentType = response.Content.Headers.ContentType?.ToString() ?? string.Empty;
                    string fileName = Path.GetFileName(new Uri(url).LocalPath);

                    if (string.IsNullOrEmpty(contentType) || string.IsNullOrEmpty(fileName))
                    {
                        return methodResult;
                    }

                    // Tải nội dung dưới dạng Stream
                    using (Stream stream = await response.Content.ReadAsStreamAsync())
                    {
                        // Tạo StreamPart và trả về
                        var streamPart = new StreamPart(stream, fileName, contentType);

                        // Call chatgpt
                        var textResult = await _openAIService.SpeechToTextByAIAsync(streamPart, Model);
                        if (!textResult.IsSuccessStatusCode)
                        {
                            methodResult.AddErrorBadRequest(textResult.Error?.Message);
                            return methodResult;
                        }

                        methodResult.Result = textResult.Content;
                        return methodResult;
                    }
                }
            }
        }
    }
}
