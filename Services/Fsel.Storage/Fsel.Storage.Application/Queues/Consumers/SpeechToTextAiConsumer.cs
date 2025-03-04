// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Storage.Application.Queues.Consumers
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base;
    using Fsel.Core.Base.Interfaces;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Models.ShareModels;
    using Fsel.Storage.Application.Queues.Publisher;
    using Fsel.Storage.Application.Services.AmazonS3Services;
    using Fsel.Storage.Application.Services.OpenAIServices;
    using Fsel.Storage.Domain.Models.EntityModels;
    using Fsel.Storage.Infrastructure.ValueSettings;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json;
    using Polly;
    using Refit;

    public class SpeechToTextAiConsumer : BaseConsumer<SpeechToTextAiConsumerModel>
    {
        private readonly IOpenAIService _openAIService;
        private readonly IAmazonS3Service _amazonS3Service;
        private readonly SpeechToTextPublisher _speechToTextPublisher;
        private readonly AppSetting _appSetting;
        private readonly ILogger<SpeechToTextAiConsumer> _logger;
        private const int Max_Time_Retry = 3;
        private readonly IDeepgramProvider _deepgramProvider;

        public SpeechToTextAiConsumer(AuthContext authContext,
                                      IHttpContextAccessor httpContextAccessor,
                                      IOpenAIService openAIService,
                                      IAmazonS3Service amazonS3Service,
                                      SpeechToTextPublisher speechToTextPublisher,
                                      AppSetting appSetting,
                                      ILogger<SpeechToTextAiConsumer> logger,
                                      IDeepgramProvider deepgramProvider) : base(authContext, httpContextAccessor)
        {
            _openAIService = openAIService;
            _amazonS3Service = amazonS3Service;
            _speechToTextPublisher = speechToTextPublisher;
            _appSetting = appSetting;
            _logger = logger;
            _deepgramProvider = deepgramProvider;
        }

        private class UserAiModel
        {
            public bool CheckSubAI { get; set; }
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

            int countRetry = 0;

            var retryAI = Policy.HandleResult<UserAiModel>(result => !result.CheckSubAI)
                                .WaitAndRetryAsync(Max_Time_Retry, retryAttempt => TimeSpan.FromSeconds(5));

            var retryResult = await retryAI.ExecuteAsync(async () =>
            {
                _logger.LogError($"CountRetry: {message.UserId} count: {countRetry += 1}");

                var stream = formFile.OpenReadStream();
                var streamPart = new StreamPart(stream, formFile.FileName, formFile.ContentType);

                var startDate = DateTime.UtcNow;

                var content = await _openAIService.SpeechToTextByAIAsync(streamPart, _appSetting.OpenAiConfig?.ApprovalAIModel);

                var endDate = DateTime.UtcNow;

                _logger.LogError($"CountTimeResponseAI: {(endDate - startDate).TotalSeconds}");
                _logger.LogError($"LogContentAI: {content.Content}");

                if (!content.IsSuccessStatusCode)
                {
                    var deepGramContent = await _deepgramProvider.GetTranscriptionAsync(formFile, "nova-2");

                    _logger.LogError($"LogContentDeepGramAI: {deepGramContent}");

                    if (string.IsNullOrEmpty(deepGramContent))
                    {
                        return new UserAiModel { CheckSubAI = false };
                    }
                    var fileInfomationDeepGram = await UpLoadFileAsync(formFile);
                    await PublishTextToSocket(message, deepGramContent, fileInfomationDeepGram.Result);
                }

                // var fileInfomation = await _amazonS3Service.UploadFileAsync(EnumBucketType.FselPublic, formFile, EnumFolderType.Videos, false, false);
                var fileInfomation = await UpLoadFileAsync(formFile);

                var convertContent = !string.IsNullOrEmpty(content.Content) ? JsonConvert.DeserializeObject<ContentModel>(content.Content)?.Text : string.Empty;
                if (string.IsNullOrEmpty(convertContent))
                {
                    return new UserAiModel { CheckSubAI = false };
                }

                // publisher real time
                //await _speechToTextPublisher.Publish(new SpeechToTextConsumerModel { UserId = message.UserId, TranscriptFile = new Shared.Models.ShareModels.TranscriptFileModel { Content = convertContent, FilePath = fileInfomation.Result } }, CancellationToken.None);

                await PublishTextToSocket(message, convertContent, fileInfomation.Result);
                return new UserAiModel { CheckSubAI = true };
            });
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

        public async Task<MethodResult<string?>> UpLoadFileAsync(IFormFile formFile)
        {
            return await _amazonS3Service.UploadFileAsync(EnumBucketType.FselPublic, formFile, EnumFolderType.Videos, false, false);
        }


        private async Task PublishTextToSocket(SpeechToTextAiConsumerModel message, string? convertContent, string? filePath)
        {
            await _speechToTextPublisher.Publish(new SpeechToTextConsumerModel { UserId = message.UserId, TranscriptFile = new Shared.Models.ShareModels.TranscriptFileModel { Content = convertContent, FilePath = filePath } }, CancellationToken.None);
        }

    }
}
