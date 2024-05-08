// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Storage.Application.Command.ChatbotCmd
{
    using System.Threading;
    using Fsel.Common.ActionResults;
    using Fsel.Shared.Enums;
    using Fsel.Storage.Application.Services.AmazonS3Services;
    using Fsel.Storage.Application.Services.OpenAIServices;
    using Fsel.Storage.Application.Services.OpenAIServices.Models;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class CreateChatbotAudioCommand : IRequest<MethodResult<string>>
    {
        public string? Text { get; set; }

        public string? Model { get; set; } = "tts-1";

        public string? Voice { get; set; } = "nova";
    }

    public class CreateChatbotAudioCommandHandler : IRequestHandler<CreateChatbotAudioCommand, MethodResult<string>>
    {
        private readonly IOpenAIService _openAIService;
        private readonly IAmazonS3Service _amazonS3Service;

        public CreateChatbotAudioCommandHandler(IOpenAIService openAIService, IAmazonS3Service amazonS3Service)
        {
            _openAIService = openAIService;
            _amazonS3Service = amazonS3Service;
        }

        public async Task<MethodResult<string>> Handle(CreateChatbotAudioCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<string>();

            var response = await _openAIService.GenerateAudioByAIAsync(new AudioChatbotModel
            {
                Model = request.Model,
                Input = request.Text,
                Voice = request.Voice
            });

            methodResult.Result = await ConvertHttpContentToIFormFile(response.Content!, "listening.mpeg");
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }

        public async Task<string> ConvertHttpContentToIFormFile(HttpContent httpContent, string fileName)
        {
            if (httpContent == null ||
                httpContent.Headers == null ||
                httpContent.Headers.ContentType == null ||
                httpContent.Headers.ContentType.MediaType == null)
            {
                return string.Empty;
            }
            // Đọc nội dung của HttpContent vào một mảng byte
            var contentBytes = await httpContent.ReadAsByteArrayAsync();

            // Tạo một MemoryStream từ mảng byte
            using (var stream = new MemoryStream(contentBytes))
            {
                // Tạo một đối tượng IFormFile từ MemoryStream
                var formFile = new FormFile(stream, 0, contentBytes.Length, null, fileName)
                {
                    Headers = new HeaderDictionary(),
                    ContentType = httpContent.Headers.ContentType.MediaType
                };

                var fileInfomation = await _amazonS3Service.UploadFileAsync(EnumBucketType.FselPublic, formFile, EnumFolderType.Files, false, true);

                if (fileInfomation.Result == null)
                {
                    return string.Empty;
                }
                return fileInfomation.Result;
            }
        }
    }
}
