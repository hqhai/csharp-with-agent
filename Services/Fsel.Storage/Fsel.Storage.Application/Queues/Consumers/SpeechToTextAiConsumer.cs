// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Storage.Application.Queues.Consumers
{
    using System.Threading.Tasks;
    using Fsel.Core.Base;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Models.ShareModels;
    using Fsel.Storage.Application.Queues.Publisher;
    using Fsel.Storage.Application.Services.AmazonS3Services;
    using Fsel.Storage.Application.Services.OpenAIServices;
    using Fsel.Storage.Domain.Models.EntityModels;
    using Microsoft.AspNetCore.Http;
    using Newtonsoft.Json;
    using Refit;

    public class SpeechToTextAiConsumer : BaseConsumer<SpeechToTextAiConsumerModel>
    {
        private readonly IOpenAIService _openAIService;
        private readonly IAmazonS3Service _amazonS3Service;
        private readonly SpeechToTextPublisher _speechToTextPublisher;
        private const string AIModel = "whisper-1";

        public SpeechToTextAiConsumer(AuthContext authContext, IHttpContextAccessor httpContextAccessor, IOpenAIService openAIService, IAmazonS3Service amazonS3Service, SpeechToTextPublisher speechToTextPublisher) : base(authContext, httpContextAccessor)
        {
            _openAIService = openAIService;
            _amazonS3Service = amazonS3Service;
            _speechToTextPublisher = speechToTextPublisher;
        }

        public async override Task ConsumeQueue(SpeechToTextAiConsumerModel? message)
        {
            if (message == null)
            {
                return;
            }

            IFormFile formFile = ConvertToIFormFile(message.FileData, message.FileName, message.ContentType);
            if (formFile == null)
            {
                return;
            }

            var stream = formFile.OpenReadStream();
            var streamPart = new StreamPart(stream, formFile.FileName, formFile.ContentType);

            var content = await _openAIService.SpeechToTextByAIAsync(streamPart, AIModel);
            if (!content.IsSuccessStatusCode)
            {
                return;
            }

            var fileInfomation = await _amazonS3Service.UploadFileAsync(EnumBucketType.FselPublic, formFile, EnumFolderType.Videos, false, false);
            var convertContent = !string.IsNullOrEmpty(content.Content) ? JsonConvert.DeserializeObject<ContentModel>(content.Content)?.Text : string.Empty;

            // publisher realtime
            await _speechToTextPublisher.Publish(new SpeechToTextConsumerModel { UserId = message.UserId, TranscriptFile = new Shared.Models.ShareModels.TranscriptFileModel { Content = convertContent, FilePath = fileInfomation.Result } }, CancellationToken.None);
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
