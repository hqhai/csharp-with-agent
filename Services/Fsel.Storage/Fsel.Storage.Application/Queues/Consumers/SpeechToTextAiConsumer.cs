// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Storage.Application.Queues.Consumers
{
    using System.Threading.Tasks;
    using Fsel.Core.Base;
    using Fsel.Shared.Models.ShareModels;
    using Fsel.Storage.Application.Command.SpeechToTextCmd.V1i2;
    using MediatR;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;

    public class SpeechToTextAiConsumer : BaseConsumer<SpeechToTextAiConsumerModel>
    {
        private readonly IMediator _mediator;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ILogger<SpeechToTextAiConsumer> _logger;
        private readonly AppSetting _appSetting;
        public SpeechToTextAiConsumer(IMediator mediator, AuthContext authContext, Microsoft.AspNetCore.Http.IHttpContextAccessor httpContextAccessor, IServiceScopeFactory serviceScopeFactory, ILogger<SpeechToTextAiConsumer> logger) : base(authContext, httpContextAccessor)
        {
            _mediator = mediator;
            _serviceScopeFactory = serviceScopeFactory;
            _logger = logger;
        }
                                      IOpenAIService openAIService,
                                      IAmazonS3Service amazonS3Service,
                                      SpeechToTextPublisher speechToTextPublisher,
                                      AppSetting appSetting,
                                      ILogger<SpeechToTextAiConsumer> logger,
                                      ICognitiveProvider cognitiveProvider) : base(authContext, httpContextAccessor)
        {
            _ = Task.Run(async () =>
            _amazonS3Service = amazonS3Service;
                using var scope = _serviceScopeFactory.CreateScope();
                var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

                _logger.LogCritical("Processing In Task Run");
                await mediator.Publish(new PublishSpeakToTextToRealTimeCommand
                {
                    UserId = message.UserId,
                    FileName = message.FileName,
                    ContentType = message.ContentType,
                    FileData = message.FileData
                });
            });

            _logger.LogCritical("Processing Out Of Task Run");

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
                    var deepGramContent = await _cognitiveProvider.GetTranscriptionAsync(formFile);

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
