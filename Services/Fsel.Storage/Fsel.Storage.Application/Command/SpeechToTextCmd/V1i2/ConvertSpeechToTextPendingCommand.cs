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

    public class ConvertSpeechToTextPendingCommand : SpeechToTextPendingAiConsumerModel, IRequest<MethodResult<bool>>
    {
    }

    public class ConvertSpeechToTextPendingCommandHandler : IRequestHandler<ConvertSpeechToTextPendingCommand, MethodResult<bool>>
    {
        private readonly IOpenAIService _openAIService;
        private readonly IAmazonS3Service _amazonS3Service;
        private readonly AppSetting _appSetting;
        private readonly IDeepgramProvider _deepgramProvider;
        private readonly ICognitiveProvider _cognitiveProvider;
        private readonly ResponseSpeechToTextPendingPublisher _responseSpeechToTextPendingPublisher;
        private readonly ILogger<ConvertSpeechToTextPendingCommand> _logger;
        private const int Max_Time_Retry = 3;
        private const int Retry_GPT_Time = 2;
        private int _countRetry;
        private int _intervalRetryTime = 5;

        public ConvertSpeechToTextPendingCommandHandler(IOpenAIService openAIService,
                                                        IAmazonS3Service amazonS3Service,
                                                        AppSetting appSetting,
                                                        IDeepgramProvider deepgramProvider,
                                                        ICognitiveProvider cognitiveProvider,
                                                        ResponseSpeechToTextPendingPublisher responseSpeechToTextPendingPublisher,
                                                        ILogger<ConvertSpeechToTextPendingCommand> logger)
        {
            _openAIService = openAIService;
            _amazonS3Service = amazonS3Service;
            _appSetting = appSetting;
            _deepgramProvider = deepgramProvider;
            _cognitiveProvider = cognitiveProvider;
            _responseSpeechToTextPendingPublisher = responseSpeechToTextPendingPublisher;
            _logger = logger;
        }

        public async Task<MethodResult<bool>> Handle(ConvertSpeechToTextPendingCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<bool> methodResult = new MethodResult<bool>();

            IFormFile formFile = ConvertToIFormFile(request.FileData, request.FileName, request.ContentType);
            if (formFile == null)
            {
                return methodResult;
            }

            var pollyRetry = Policy.HandleResult<bool>(result => !result)
                                   .WaitAndRetryAsync(Max_Time_Retry, retryAttempt => TimeSpan.FromSeconds(_intervalRetryTime));

            var retryResult = await pollyRetry.ExecuteAsync(async () =>
            {
                var stream = formFile.OpenReadStream();
                var streamPart = new StreamPart(stream, formFile.FileName, formFile.ContentType);

                string contentText = null; // Lưu trữ nội dung từ OpenAI

                if (_countRetry <= Retry_GPT_Time)
                {
                    var content = await _openAIService.SpeechToTextByAIAsync(streamPart, _appSetting.OpenAiConfig?.ApprovalAIModel);
                    contentText = !string.IsNullOrEmpty(content.Content) ? JsonConvert.DeserializeObject<ContentModel>(content.Content)?.Text : string.Empty;
                }

                // Nếu OpenAI thất bại hoặc hết retry, chuyển sang DeepGram
                if (string.IsNullOrEmpty(contentText) || _countRetry == Max_Time_Retry)
                {
                    var deepGramContent = await _deepgramProvider.GetTranscriptionAsync(formFile);
                    _logger.LogError($"LogContentDeepgramPendingSTT: userId: {request.UserId} classForumDetailResultId: {request.ClassForumDetailResultId} content: {deepGramContent} date: {DateTime.UtcNow}");

                    if (string.IsNullOrEmpty(deepGramContent))
                    {
                        // vào azure
                        contentText = await _cognitiveProvider.GetTranscriptionAsync(formFile);
                        _logger.LogError($"LogContentAzurePendingSTT: userId: {request.UserId} classForumDetailResultId: {request.ClassForumDetailResultId} content: {contentText} date: {DateTime.UtcNow}");
                    }
                    else
                    {
                        contentText = deepGramContent;
                    }

                    if (string.IsNullOrEmpty(contentText))
                    {
                        await SendResponseSpeechToText(formFile, request.ClassForumDetailResultId, contentText, request.UserId, cancellationToken);
                    }
                }

                if (string.IsNullOrEmpty(contentText))
                {
                    _countRetry += 1;
                    return false;
                }
                else
                {
                    await SendResponseSpeechToText(formFile, request.ClassForumDetailResultId, contentText, request.UserId, cancellationToken);
                    _logger.LogError($"LogContentChatGptPendingSTT: userId: {request.UserId} classForumDetailResultId: {request.ClassForumDetailResultId} content: {contentText}  date: {DateTime.UtcNow}");
                }

                return true;
            });

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

        public async Task<MethodResult<string?>> UpLoadFileAsync(IFormFile formFile)
        {
            return await _amazonS3Service.UploadFileAsync(EnumBucketType.FselPublic, formFile, EnumFolderType.Videos, false, false);
        }

        private async Task SendResponseSpeechToText(IFormFile formFile, Guid classForumDetailResultId, string? contentText, Guid userId, CancellationToken cancellationToken)
        {
            _logger.LogError($"LogContentPendingSTT: userId: {userId} classForumDetailResultId: {classForumDetailResultId} content: {contentText}  date: {DateTime.UtcNow}");
            await _responseSpeechToTextPendingPublisher.Publish(new ResponseSpeechToTextPendingAiConsumerModel
            {
                ClassForumDetailResultId = classForumDetailResultId,
                WordContent = contentText
            }, cancellationToken);
        }
    }
}
