// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Commands.Chatbots
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Common.Helpers;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Helpers;
    using Fsel.Shared.Models.ShareModels;
    using Fsel.System.Application.Queues.Publisher;
    using Fsel.System.Application.Services.AIServices;
    using Fsel.System.Application.Services.AIServices.Models;
    using Fsel.System.Application.Services.CourseServices;
    using Fsel.System.Application.Services.CourseServices.Models;
    using Fsel.System.Application.Services.CourseServices.QueryModels;
    using Fsel.System.Application.Services.StorageServices;
    using Fsel.System.Application.Services.StorageServices.Models;
    using Fsel.System.Domain.Entities.Chatbots;
    using Fsel.System.Domain.Enums;
    using Fsel.System.Domain.Enums.ErrorCodes;
    using Fsel.System.Domain.IRepositories;
    using Fsel.System.Domain.Models.CommandModels.ChatBot;
    using Fsel.System.Domain.Models.EntityModels;
    using global::System.ComponentModel.DataAnnotations;
    using global::System.Text.RegularExpressions;
    using global::System.Threading;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;

    public class SaveChatBotMessageCommand : IRequest<MethodResult<ChatBotModel>>
    {
        [MaxLength(4000, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? Content { get; set; }

        public Guid ChatBotId { get; set; }
    }

    public class SaveChatBotMessageCommandHandler : IRequestHandler<SaveChatBotMessageCommand, MethodResult<ChatBotModel>>
    {
        private readonly IMapper _mapper;
        private readonly IChatBotRepository _chatBotRepository;
        private readonly IChatbotConfigRepository _chatbotConfigRepository;
        private readonly IOpenAIService _openAIService;
        private readonly IStorageService _storageService;
        private readonly ChatBotPublisher _chatBotPublisher;
        private const int Number_Of_Config = 2;
        private readonly ILogger<SaveChatBotMessageCommandHandler> _logger;
        private readonly IMediator _mediator;
        private readonly ICourseService _courseService;

        public SaveChatBotMessageCommandHandler(IMapper mapper,
            IChatBotRepository chatBotRepository,
            IStorageService storageService,
            ChatBotPublisher chatBotPublisher,
            IChatbotConfigRepository chatbotConfigRepository,
            IOpenAIService openAIService,
            ILogger<SaveChatBotMessageCommandHandler> logger,
            IMediator mediator,
            ICourseService courseService)
        {
            _mapper = mapper;
            _chatBotRepository = chatBotRepository;
            _storageService = storageService;
            _chatBotPublisher = chatBotPublisher;
            _chatbotConfigRepository = chatbotConfigRepository;
            _openAIService = openAIService;
            _logger = logger;
            _mediator = mediator;
            _courseService = courseService;
        }

        public async Task<MethodResult<ChatBotModel>> Handle(SaveChatBotMessageCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<ChatBotModel>();

            var chatbotMessage = await GetChatBotAsync(request.ChatBotId, methodResult, cancellationToken);
            if (!methodResult.IsOK)
            {
                return methodResult;
            }

            var chatbotSkillConfig = await GetSkillConfigAsync(chatbotMessage, methodResult, cancellationToken);
            if (!methodResult.IsOK)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(chatbotSkillConfig));
                return methodResult;
            }

            if (chatbotMessage.RemainToken == 0)
            {
                methodResult.AddErrorBadRequest(nameof(EnumOutOfAIToken.TheNumberOfTokensHasReachedTheLimit));
                return methodResult;
            }
            // 3️⃣ Load AI criteria config
            var aiConfig = await LoadAiCriteriaConfigAsync(chatbotSkillConfig?.AICriteriaConfigId);

            #region chatgpt

            ///Bổ sung câu hỏi của học sinh vào đoạn hội thoại
            ChatbotResponseModel newQuestion = CompletionElement("user", request.Content);
            var chatBotMessageModel = _mapper.Map<IList<ChatBotMessageModel>>(chatbotMessage.Conversations);
            chatBotMessageModel.Add(newQuestion);

            var chatGptResponse = await _openAIService.SubmitAICompletionsAsync(new RequestAIModel
            {
                Model = aiConfig?.AiModel ?? ValueSettings.ChatBotSetup.Model,
                Messages = chatBotMessageModel,
                Temperature = aiConfig?.SettingTemperature ?? ValueSettings.ChatBotSetup.Temperature,
                MaxTokens = chatbotMessage.RemainToken,
                PresencePenalty = aiConfig?.SettingPresence ?? ValueSettings.ChatBotSetup.PresencePenalty,
                TopP = aiConfig?.SettingTopP ?? ValueSettings.ChatBotSetup.TopP
            });

            string response = chatGptResponse?.Content?.Choices?.Select(x => x.Message?.Content).FirstOrDefault() ?? string.Empty;
            response = Shared.Helpers.StringHelper.TextCleaner.NormalizeListeningContent(response);

            #endregion chatgpt

            string tokenInUse = chatGptResponse?.Content?.Usage?.ToString() ?? string.Empty;
            var totalTokenUse = ConvertHelper.Deserialize<TokenAIModel>(tokenInUse);
            bool isContainAudioScript = response.Contains("Click to listen", StringComparison.OrdinalIgnoreCase);
            string filePath = await TextToSpeech(chatbotSkillConfig.ChatbotLayout, isContainAudioScript, response);

            // Bổ sung câu trả lời của GPT vào đoạn hội thoại
            var chatBotResponse = _mapper.Map<List<ChatbotResponseModel>>(chatbotMessage.Conversations);
            chatBotResponse.Add(newQuestion);
            ChatbotResponseModel newMessage = CompletionElement("system", response, filePath);
            chatBotResponse.Add(newMessage);

            //Tính toán số token đã sử dụng
            chatbotMessage.Conversations = _mapper.Map<List<ChatBotMessage>>(chatBotResponse);
            chatbotMessage.LastestAnswer = _mapper.Map<ChatBotMessage>(newMessage);
            MatchCollection matches = Regex.Matches(response ?? string.Empty, @"\b\w+\b");

            int tokenCount = matches.Count + (totalTokenUse?.Completion_Tokens ?? default);
            chatbotMessage.RemainToken = chatbotMessage.RemainToken > tokenCount ? chatbotMessage.RemainToken - tokenCount : 0;
            var tokenRatio = CalculateTokenRatio(chatbotMessage.RemainToken, chatbotSkillConfig.Token);

            // Push to Socket
            await PushToWebSocket(request.ChatBotId, newMessage.Content, newMessage.FilePath, tokenRatio, cancellationToken);

            //Lưu đoạn hội thoại vào database
            await _chatBotRepository.ExecuteTransactionAsync(async () =>
            {
                if (chatbotMessage.RemainToken == 0)
                {
                    chatbotMessage.Status = EnumChatBotStatus.Done;
                }
                //Câp nhật xuống database
                _chatBotRepository.Update(chatbotMessage);
                await _chatBotRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

                chatbotMessage.Conversations = RemoveFirstTwoElements(chatbotMessage.Conversations);
                methodResult.StatusCode = StatusCodes.Status201Created;
                methodResult.Result = _mapper.Map<ChatBotModel>(chatbotMessage);
                return methodResult;
            });

            return methodResult;
        }

        private async Task<AICriteriaConfigsModel?> LoadAiCriteriaConfigAsync(Guid? configId)
        {
            if (!configId.HasValue)
            {
                return null;
            }

            var result = await _courseService.GetConfigByIdAsync(
                new GetAICriteriaConfigsQueryModel
                {
                    Id = configId.Value
                });

            return result?.Content?.Result;
        }

        private async Task<ChatBot> GetChatBotAsync(Guid chatBotId, MethodResult<ChatBotModel> result, CancellationToken ct)
        {
            var chatbot = await _chatBotRepository.Queryable
                .FirstOrDefaultAsync(x => x.Id == chatBotId, ct);

            if (chatbot == null)
            {
                result.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                return new ChatBot();
            }
            return chatbot;
        }

        private async Task<ChatbotSkillConfig> GetSkillConfigAsync(
        ChatBot chatbot,
        MethodResult<ChatBotModel> result,
        CancellationToken ct)
        {
            var config = await _chatbotConfigRepository.Queryable
                .Include(x => x.ChatbotSkillConfigs)
                .FirstOrDefaultAsync(x => x.UnitId == chatbot.UnitId, ct);

            if (config == null)
            {
                result.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(ChatbotConfig));
                return new ChatbotSkillConfig();
            }

            var skill = config.ChatbotSkillConfigs.FirstOrDefault(x => x.SkillId == chatbot.SkillId);

            if (skill == null)
            {
                result.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(ChatbotSkillConfig));
                return new ChatbotSkillConfig();
            }
            return skill;
        }

        #region Func

        /// <summary>
        /// Bỏ đi các phần tử config ở đầu mảng
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <returns></returns>
        private static IList<T> RemoveFirstTwoElements<T>(IList<T> list)
        {
            if (list.Count >= Number_Of_Config)
            {
                return list.Skip(2).ToList();
            }
            else
            {
                // Danh sách rỗng, trả về danh sách rỗng
                return new List<T>();
            }
        }

        /// <summary>
        /// Tính toán số lượng token còn lại
        /// </summary>
        /// <param name="remainToken"></param>
        /// <param name="skill"></param>
        /// <param name="chatbotConfig"></param>
        /// <returns></returns>
        private static double CalculateTokenRatio(double remainToken, double token = default)
        {
            return (double)remainToken / token;
        }

        /// <summary>
        /// Chuyển Text sang Audio
        /// </summary>
        /// <param name="skill"></param>
        /// <param name="isContainAudioScript"></param>
        /// <param name="script"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task<string> TextToSpeech(EnumChatbotLayout layout, bool isContainAudioScript, string? script)
        {
            string filePath = string.Empty;
            if (layout == EnumChatbotLayout.Listening && isContainAudioScript)
            {
                string scriptListening = ExtractTranscript(script);
                var audioResult = await _storageService.TextToSpeech(new CreateChatbotAudioModel
                {
                    Text = scriptListening,
                    Model = "tts-1",
                    Voice = "nova",
                });
                filePath = audioResult?.Content?.Result!;

                try
                {
                    var reponse = await _mediator.Send(new ConvertFileWavCommand { File = filePath }, CancellationToken.None);
                    if (!reponse.IsOK)
                    {
                        _logger.LogError($"ConvertFileWavCommand : {filePath} => {reponse.Result}", reponse.ErrorMessages);
                    }
                    filePath = reponse.Result ?? filePath;
                }
                catch (Exception ex)
                {
                    _logger.LogError($"ConvertFileWavCommand Exception : {filePath} ", ex.Message);
                }
            }
            return filePath;
        }

        /// <summary>
        /// Push message to websocket
        /// </summary>
        /// <param name="chatBotId"></param>
        /// <param name="message"></param>
        /// <param name="filePath"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task PushToWebSocket(Guid chatBotId, string? message, string? filePath, double tokenRation, CancellationToken cancellationToken)
        {
            ChatBotSendingMessageModel model = new ChatBotSendingMessageModel
            {
                ChatbotId = chatBotId,
                Content = message,
                FilePath = filePath,
                TokenRatio = tokenRation
            };
            _logger.LogInformation($"Feature Send to ChatbotPublisher:{ConvertHelper.Serialize(model)}, Environment.MachineName: {Environment.MachineName}");
            await _chatBotPublisher.Publish(model, cancellationToken);
        }

        /// <summary>
        /// Khởi tạo mẫu câu message để chat với gpt
        /// </summary>
        /// <param name="role"></param>
        /// <param name="content"></param>
        /// <param name="filePath"></param>
        /// <returns></returns>
        private static ChatbotResponseModel CompletionElement(string? role, string? content, string? filePath = null)
        {
            return new ChatbotResponseModel
            {
                Role = role,
                Content = content,
                FilePath = filePath
            };
        }

        /// <summary>
        /// Get Content of Listening from ChatGPT
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static string ExtractTranscript(string? text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return string.Empty;
            }

            // Sử dụng regex để tìm và lấy nội dung trong dấu ngoặc nhọn {}
            Match match = Regex.Match(text, @"Click to listen:\s*.*?\s*{\s*(.*?)\s*}");
            if (match.Success)
            {
                // Lấy nội dung trong dấu ngoặc nhọn
                string transcript = match.Groups[1].Value.Trim();
                return transcript;
            }
            else
            {
                return string.Empty;
            }
        }

        #endregion Func
    }
}
