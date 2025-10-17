// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Storage.Application.Command.SpeechToTextCmd.V1i2
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base.Interfaces;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Models.ShareModels;
    using Fsel.Storage.Application.Queues.Publisher;
    using Fsel.Storage.Application.Services.AmazonS3Services;
    using Fsel.Storage.Application.Services.OpenAIServices;
    using Fsel.Storage.Domain.Models.EntityModels;
    using Fsel.Storage.Infrastructure.ValueSettings;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json;
    using Polly;
    using Refit;

    public class PublishSpeakToTextToRealTimeCommand : SpeechToTextAiConsumerModel, INotification
    {
    }

    public class PublishSpeakToTextToRealTimeCommandHandler : INotificationHandler<PublishSpeakToTextToRealTimeCommand>
    {
        private readonly IOpenAIService _openAIService;
        private readonly IAmazonS3Service _amazonS3Service;
        private readonly SpeechToTextPublisher _speechToTextPublisher;
        private readonly AppSetting _appSetting;
        private readonly ILogger<PublishSpeakToTextToRealTimeCommand> _logger;
        private const int Max_Time_Retry = 3;
        private const int Retry_GPT_Time = 2;
        private readonly IDeepgramProvider _deepgramProvider;
        private readonly ICognitiveProvider _cognitiveProvider;
        private readonly IMediator _mediator;
        private int _countRetry;
        private int _intervalRetryTime = 5;
        private DateTime _startDate, _endDate;

        public PublishSpeakToTextToRealTimeCommandHandler(IOpenAIService openAIService,
                                                          IAmazonS3Service amazonS3Service,
                                                          SpeechToTextPublisher speechToTextPublisher,
                                                          AppSetting appSetting,
                                                          ILogger<PublishSpeakToTextToRealTimeCommand> logger,
                                                          IDeepgramProvider deepgramProvider,
                                                          ICognitiveProvider cognitiveProvider,
                                                          IMediator mediator)
        {
            _openAIService = openAIService;
            _amazonS3Service = amazonS3Service;
            _speechToTextPublisher = speechToTextPublisher;
            _appSetting = appSetting;
            _logger = logger;
            _deepgramProvider = deepgramProvider;
            _cognitiveProvider = cognitiveProvider;
            _mediator = mediator;
        }

        public async Task Handle(PublishSpeakToTextToRealTimeCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            #region Convert file ffmpeg
            IFormFile formFileDefault = ConvertToIFormFile(request.FileData, request.FileName, request.ContentType);
            if (formFileDefault == null)
            {
                return;
            }

            var convertFile = await _mediator.Send(new CheckFileAndConvertCommand { FormFile = formFileDefault }, cancellationToken);
            if (!convertFile.IsOK || convertFile.Result == null)
            {
                return;
            }

            IFormFile formFile = convertFile.Result;
            if (formFile == null)
            {
                return;
            }
            #endregion

            var pollyRetry = Policy.HandleResult<bool>(result => !result)
                                .WaitAndRetryAsync(Max_Time_Retry, retryAttempt => TimeSpan.FromSeconds(_intervalRetryTime));

            var retryResult = await pollyRetry.ExecuteAsync(async () =>
            {
                _logger.LogError($"CountRetry: {request.UserId} count: {_countRetry += 1}");

                var stream = formFile.OpenReadStream();
                var streamPart = new StreamPart(stream, formFile.FileName, formFile.ContentType);
                try
                {
                    string contentText = null; // Lưu trữ nội dung từ OpenAI

                    try
                    {
                        if (_countRetry <= Retry_GPT_Time)
                        {
                            _startDate = DateTime.UtcNow;
                            var content = await _openAIService.SpeechToTextByAIAsync(streamPart, _appSetting.OpenAiConfig?.ApprovalAIModel);
                            contentText = !string.IsNullOrEmpty(content.Content) ? JsonConvert.DeserializeObject<ContentModel>(content.Content)?.Text : string.Empty;
                            _endDate = DateTime.UtcNow;
                            _logger.LogError($"CountTimeResponseAI: {(_endDate - _startDate).TotalSeconds}");
                            _logger.LogError($"LogContentAI: {content.Content}");
                        }
                    }
                    catch (TaskCanceledException) // Bắt timeout riêng
                    {
                        _logger.LogError("OpenAI bị timeout, tiếp tục với DeepGram...");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Lỗi khác khi gọi OpenAI: {ex}");
                    }

                    // Nếu OpenAI thất bại hoặc hết retry, chuyển sang DeepGram
                    if (string.IsNullOrEmpty(contentText) || _countRetry == Max_Time_Retry)
                    {
                        var deepGramContent = await _deepgramProvider.GetTranscriptionAsync(formFile);
                        _logger.LogError($"LogContentDeepGramAI: {deepGramContent}");

                        if (string.IsNullOrEmpty(deepGramContent))
                        {
                            var cognitiveContent = await _cognitiveProvider.GetTranscriptionAsync(formFile);
                            _logger.LogError($"LogContentCognitiveAI: {cognitiveContent}");
                            var fileInFomationCognitive = await UpLoadFileAsync(formFile);
                            await PublishTextToSocket(request, cognitiveContent, fileInFomationCognitive.Result);
                            return true;
                        }

                        var fileInfomationDeepGram = await UpLoadFileAsync(formFile);
                        await PublishTextToSocket(request, deepGramContent, fileInfomationDeepGram.Result);
                        return true;
                    }

                    if (string.IsNullOrEmpty(contentText))
                    {
                        return false;
                    }

                    var fileInfomation = await UpLoadFileAsync(formFile);
                    await PublishTextToSocket(request, contentText, fileInfomation.Result);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"TimeOut: {_countRetry}, Error: {ex}");
                }

                return true;

            });

            return;
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
            var a = await _amazonS3Service.UploadFileAsync(EnumBucketType.FselPublic, formFile, EnumFolderType.Videos, false, false);
            return a;
        }

        private async Task PublishTextToSocket(SpeechToTextAiConsumerModel message, string? convertContent, string? filePath)
        {
            await _speechToTextPublisher.Publish(new SpeechToTextConsumerModel { UserId = message.UserId, TranscriptFile = new Shared.Models.ShareModels.TranscriptFileModel { Content = convertContent, FilePath = filePath, DateTime = message.CurrentDate } }, CancellationToken.None);
        }
    }
}
