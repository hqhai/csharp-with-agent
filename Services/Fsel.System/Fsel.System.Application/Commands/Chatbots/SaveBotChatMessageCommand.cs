// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Commands.Chatbots
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Common.Helpers;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Enums;
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
    using Fsel.System.Domain.Enums.ErrorCodes;
    using Fsel.System.Domain.IRepositories;
    using Fsel.System.Domain.Models.CommandModels.ChatBot;
    using Fsel.System.Domain.Models.EntityModels;
    using global::System.Text.RegularExpressions;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;

    public class SaveBotChatMessageCommand : IRequest<MethodResult<ChatBotModel>>
    {
        public Guid ChatBotId { get; set; }
    }

    public class SaveBotChatMessageCommandHandler : IRequestHandler<SaveBotChatMessageCommand, MethodResult<ChatBotModel>>
    {
        private readonly IMapper _mapper;
        private readonly IChatBotRepository _chatBotRepository;
        private readonly IChatbotConfigRepository _chatbotConfigRepository;
        private readonly IOpenAIService _openAIService;
        private readonly IStorageService _storageService;
        private readonly ChatBotPublisher _chatBotPublisher;
        private readonly ILogger<SaveBotChatMessageCommandHandler> _logger;
        private readonly IMediator _mediator;
        private readonly ICourseService _courseService;
        private const int ConfigMessageCount = 2; // nếu có thể, thay bằng flag/role-based filtering thay vì hardcode

        public SaveBotChatMessageCommandHandler(
            IMapper mapper,
            IChatBotRepository chatBotRepository,
            IStorageService storageService,
            ChatBotPublisher chatBotPublisher,
            IChatbotConfigRepository chatbotConfigRepository,
            IOpenAIService openAIService,
            ILogger<SaveBotChatMessageCommandHandler> logger,
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

        public async Task<MethodResult<ChatBotModel>> Handle(SaveBotChatMessageCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            var result = new MethodResult<ChatBotModel>();

            var chatbot = await GetChatBotOrFailAsync(request.ChatBotId, result, cancellationToken);
            if (!result.IsOK)
            {
                return result;
            }
            var skillConfig = await GetSkillConfigOrFailAsync(chatbot, result, cancellationToken);
            if (!result.IsOK)
            {
                return result;
            }
            EnsureTokenAvailableOrFail(chatbot, result);
            if (!result.IsOK)
            {
                return result;
            }

            var aiConfig = await LoadAiCriteriaConfigAsync(skillConfig.AICriteriaConfigId, cancellationToken);

            var aiResponse = await SubmitToOpenAiAsync(chatbot, aiConfig, cancellationToken);

            var responseText = NormalizeResponse(aiResponse);

            var isContainAudioScript = responseText.Contains("Click to listen", StringComparison.OrdinalIgnoreCase);
            var audioPath = await TryGenerateAudioAsync(skillConfig.ChatbotLayout, isContainAudioScript, responseText, cancellationToken);

            var assistantMsg = CreateConversationMessage("system", responseText, audioPath);
            AppendAssistantMessage(chatbot, assistantMsg);

            ApplyTokenUsage(chatbot, aiResponse);

            var tokenRatio = CalculateTokenRatio(chatbot.RemainToken, skillConfig.Token);

            await PersistChatbotAsync(chatbot, result, cancellationToken);

            await PushToWebSocket(chatbot.Id, assistantMsg.Content, assistantMsg.FilePath, tokenRatio, cancellationToken);

            chatbot.Conversations = TrimConfigMessages(chatbot.Conversations ?? new List<ChatBotMessage>());

            result.StatusCode = StatusCodes.Status201Created;
            result.Result = _mapper.Map<ChatBotModel>(chatbot);
            return result;
        }

        // -------------------------
        // Data loading / validation
        // -------------------------

        private async Task<ChatBot> GetChatBotOrFailAsync(Guid chatBotId, MethodResult<ChatBotModel> result, CancellationToken ct)
        {
            var chatbot = await _chatBotRepository.Queryable.FirstOrDefaultAsync(x => x.Id == chatBotId, ct);
            if (chatbot == null)
            {
                result.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(ChatBot));
                return new ChatBot();
            }
            return chatbot;
        }

        private async Task<ChatbotSkillConfig> GetSkillConfigOrFailAsync(ChatBot chatbot, MethodResult<ChatBotModel> result, CancellationToken ct)
        {
            var config = await _chatbotConfigRepository.ReadQueryable
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

        private static void EnsureTokenAvailableOrFail(ChatBot chatbot, MethodResult<ChatBotModel> result)
        {
            if (chatbot.RemainToken <= 0)
            {
                result.AddErrorBadRequest(nameof(EnumOutOfAIToken.TheNumberOfTokensHasReachedTheLimit));
            }
        }

        private async Task<AICriteriaConfigsModel?> LoadAiCriteriaConfigAsync(Guid? configId, CancellationToken ct)
        {
            if (!configId.HasValue)
            {
                return null;
            }
            var response = await _courseService.GetConfigByIdAsync(new GetAICriteriaConfigsQueryModel { Id = configId.Value });
            return response?.Content?.Result;
        }

        private async Task<AIResponseModel?> SubmitToOpenAiAsync(ChatBot chatbot, AICriteriaConfigsModel? aiConfig, CancellationToken ct)
        {
            var messages = _mapper.Map<IList<ChatBotMessageModel>>(chatbot.Conversations);

            var req = new RequestAIModel
            {
                Model = aiConfig?.AiModel ?? ValueSettings.ChatBotSetup.Model,
                Messages = messages,
                Temperature = aiConfig?.SettingTemperature ?? ValueSettings.ChatBotSetup.Temperature,
                MaxTokens = chatbot.RemainToken,
                PresencePenalty = aiConfig?.SettingPresence ?? ValueSettings.ChatBotSetup.PresencePenalty,
                TopP = aiConfig?.SettingTopP ?? ValueSettings.ChatBotSetup.TopP
            };

            // Nếu OpenAI service của bạn support ct, hãy truyền ct vào trong đó.
            return (await _openAIService.SubmitAICompletionsAsync(req)).Content;
        }

        private static string NormalizeResponse(AIResponseModel? aiResponse)
        {
            var raw = aiResponse?.Choices?.Select(x => x.Message?.Content).FirstOrDefault() ?? string.Empty;
            return Shared.Helpers.StringHelper.TextCleaner.NormalizeListeningContent(raw);
        }

        private async Task<string> TryGenerateAudioAsync(EnumChatbotLayout layout, bool isContainAudioScript, string? script, CancellationToken ct)
        {
            if (layout != EnumChatbotLayout.Listening || !isContainAudioScript)
            {
                return string.Empty;
            }
            var transcript = ExtractTranscript(script);
            if (string.IsNullOrWhiteSpace(transcript))
            {
                return string.Empty;
            }
            var audioResult = await _storageService.TextToSpeech(new CreateChatbotAudioModel
            {
                Text = transcript,
                Model = "tts-1",
                Voice = "nova",
            });

            var filePath = audioResult?.Content?.Result ?? string.Empty;
            if (string.IsNullOrEmpty(filePath))
            {
                return string.Empty;
            }
            try
            {
                var convert = await _mediator.Send(new ConvertFileWavCommand { File = filePath }, ct);
                if (!convert.IsOK)
                {
                    _logger.LogError("ConvertFileWavCommand failed: {FilePath}. Errors: {Errors}",
                        filePath,
                        ConvertHelper.Serialize(convert.ErrorMessages));
                    return filePath;
                }

                return convert.Result ?? filePath;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ConvertFileWavCommand exception: {FilePath}", filePath);
                return filePath;
            }
        }

        private static ChatbotResponseModel CreateConversationMessage(string role, string content, string? filePath = null)
        {
            return new ChatbotResponseModel
            {
                Role = role,
                Content = content,
                FilePath = filePath
            };
        }

        private void AppendAssistantMessage(ChatBot chatbot, ChatbotResponseModel assistant)
        {
            var mapped = _mapper.Map<List<ChatbotResponseModel>>(chatbot.Conversations ?? new List<ChatBotMessage>());
            mapped.Add(assistant);

            chatbot.Conversations = _mapper.Map<List<ChatBotMessage>>(mapped);
            chatbot.LastestAnswer = _mapper.Map<ChatBotMessage>(assistant);
        }

        private static void ApplyTokenUsage(ChatBot chatbot, AIResponseModel? aiResponse)
        {
            // Ưu tiên usage.total_tokens từ provider (chuẩn hơn regex).
            // Tùy model/service, Usage có thể null => fallback minimal.
            var usageJson = aiResponse?.Usage?.ToString() ?? string.Empty;
            var usage = ConvertHelper.Deserialize<TokenAIModel>(usageJson);

            // Fallback: đếm từ để trừ token (không chuẩn) – chỉ để tránh “không trừ gì”
            var content = chatbot.LastestAnswer?.Content ?? string.Empty;
            var matches = Regex.Matches(content, @"\b\w+\b").Count;
            var used = matches + (usage?.Completion_Tokens ?? 0);

            chatbot.RemainToken = Math.Max(0, chatbot.RemainToken - used);
            if (chatbot.RemainToken == 0)
            {
                chatbot.Status = EnumChatBotStatus.Done;
            }
        }

        private async Task PersistChatbotAsync(ChatBot chatbot, MethodResult<ChatBotModel> methodResult, CancellationToken ct)
        {
            await _chatBotRepository.ExecuteTransactionAsync(async () =>
            {
                _chatBotRepository.Update(chatbot);
                await _chatBotRepository.UnitOfWork.SaveChangesAsync(ct).ConfigureAwait(false);
                return methodResult;
            });
        }

        private async Task PushToWebSocket(Guid chatBotId, string? message, string? filePath, double tokenRatio, CancellationToken ct)
        {
            var model = new ChatBotSendingMessageModel
            {
                ChatbotId = chatBotId,
                Content = message,
                FilePath = filePath,
                TokenRatio = tokenRatio
            };

            _logger.LogInformation("Send to ChatbotPublisher: {Payload}, Machine: {Machine}",
                ConvertHelper.Serialize(model),
                Environment.MachineName);

            await _chatBotPublisher.Publish(model, ct);
        }

        private static double CalculateTokenRatio(double remainToken, double token)
            => token <= 0 ? 0 : remainToken / token;

        private static IList<ChatBotMessage> TrimConfigMessages(IList<ChatBotMessage> conversations)
        {
            if (conversations == null || conversations.Count == 0)
            {
                return new List<ChatBotMessage>();
            }
            if (conversations.Count <= ConfigMessageCount)
            {
                return new List<ChatBotMessage>();
            }
            return conversations.Skip(ConfigMessageCount).ToList();
        }

        public static string ExtractTranscript(string? text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return string.Empty;
            }
            var match = Regex.Match(text, @"Click to listen:\s*.*?\s*{\s*(.*?)\s*}", RegexOptions.Singleline);
            return match.Success ? match.Groups[1].Value.Trim() : string.Empty;
        }
    }
}
