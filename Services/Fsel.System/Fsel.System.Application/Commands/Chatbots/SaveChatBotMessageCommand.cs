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
    using Fsel.System.Application.Services.StorageServices;
    using Fsel.System.Application.Services.StorageServices.Models;
    using Fsel.System.Domain.Entities.ChatBot;
    using Fsel.System.Domain.Entities.Chatbots;
    using Fsel.System.Domain.Enums.ErrorCodes;
    using Fsel.System.Domain.IRepositories;
    using Fsel.System.Domain.Models.CommandModels.ChatBot;
    using Fsel.System.Domain.Models.EntityModels;
    using global::System.ComponentModel.DataAnnotations;
    using global::System.Text.RegularExpressions;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;

    public class SaveChatBotMessageCommand : SaveChatBotMessageModel, IRequest<MethodResult<ChatBotModel>>
    {
        [MaxLength(4000, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? Content { get; set; }

        public Guid? ChatBotId { get; set; }
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
        private readonly ILogger<object> _logger;

        public SaveChatBotMessageCommandHandler(IMapper mapper, IChatBotRepository chatBotRepository, IStorageService storageService, ChatBotPublisher chatBotPublisher, IChatbotConfigRepository chatbotConfigRepository, IOpenAIService openAIService, ILogger<SaveChatBotMessageCommandHandler> logger)
        {
            _mapper = mapper;
            _chatBotRepository = chatBotRepository;

            _storageService = storageService;
            _chatBotPublisher = chatBotPublisher;
            _chatbotConfigRepository = chatbotConfigRepository;
            _openAIService = openAIService;
            _logger = logger;
        }

        public async Task<MethodResult<ChatBotModel>> Handle(SaveChatBotMessageCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<ChatBotModel>();

            var chatbotMessage = _chatBotRepository.Queryable.FirstOrDefault(x => x.Id == request.ChatBotId);

            if (chatbotMessage == null)
            {
                methodResult.AddError(nameof(EnumSystemErrorCode.DataNotExist));
                return methodResult;
            }

            var chatbotConfig = _chatbotConfigRepository.Queryable.Include(x => x.ChatbotSkillConfigs).Include(x => x.ChatbotTokenConfigs).FirstOrDefault(x => x.UnitId == chatbotMessage.UnitId);

            if (chatbotConfig == null)
            {
                methodResult.AddError(nameof(EnumSystemErrorCode.DataNotExist));
                return methodResult;
            }

            double tokenRatio = CalculateTokenRatio(chatbotMessage.RemainToken, chatbotMessage.Skill, chatbotConfig);

            // Khi token còn dưới 20% so với số lượng token ban đầu
            if (chatbotMessage.RemainToken == 0)
            {
                methodResult.AddError(nameof(EnumOutOfAIToken.TheNumberOfTokensHasReachedTheLimit));
                return methodResult;
            }

            ///Bổ sung câu hỏi của học sinh vào đoạn hội thoại
            ChatbotResponseModel newQuestion = CompletionElement("user", request.Content);
            var chatBotMessageModel = _mapper.Map<IList<ChatBotMessageModel>>(chatbotMessage.Conversations);
            chatBotMessageModel.Add(newQuestion);

            var chatGptResponse = await _openAIService.SubmitAICompletionsAsync(new RequestAIModel
            {
                Model = ValueSettings.ChatBotSetup.Model,
                Messages = chatBotMessageModel,
                Temperature = ValueSettings.ChatBotSetup.Temperature,
                MaxTokens = chatbotMessage.RemainToken,
                PresencePenalty = ValueSettings.ChatBotSetup.PresencePenalty,
                TopP = ValueSettings.ChatBotSetup.TopP
            });

            string response = chatGptResponse?.Content?.Choices?.Select(x => x.Message?.Content).FirstOrDefault() ?? string.Empty;
            string tokenInUse = chatGptResponse?.Content?.Usage?.ToString() ?? string.Empty;
            var totalTokenUse = ConvertHelper.Deserialize<TokenAIModel>(tokenInUse);
            bool isContainAudioScript = response.Contains("Click to listen", StringComparison.OrdinalIgnoreCase);
            string filePath = await TextToSpeech(chatbotMessage.Skill, isContainAudioScript, response);

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
            tokenRatio = CalculateTokenRatio(chatbotMessage.RemainToken, chatbotMessage.Skill, chatbotConfig);

            // Push to Socket
            await PushToWebSocket(request.ChatBotId, newMessage.Content, newMessage.FilePath, tokenRatio, cancellationToken);

            //Lưu đoạn hội thoại vào database
            ChatBot chatBot = new ChatBot();
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

            methodResult.StatusCode = StatusCodes.Status201Created;
            return methodResult;
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
        private static double CalculateTokenRatio(double remainToken, EnumCourseSkill skill, ChatbotConfig chatbotConfig)
        {
            return (double)remainToken / GetSkillToken(skill, chatbotConfig);
        }

        /// <summary>
        /// Chuyển Text sang Audio
        /// </summary>
        /// <param name="skill"></param>
        /// <param name="isContainAudioScript"></param>
        /// <param name="script"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task<string> TextToSpeech(EnumCourseSkill skill, bool isContainAudioScript, string? script)
        {
            string filePath = string.Empty;
            if (skill == EnumCourseSkill.Listening && isContainAudioScript)
            {
                string scriptListening = ExtractTranscript(script);
                var audioResult = await _storageService.TextToSpeech(new CreateChatbotAudioModel
                {
                    Text = scriptListening,
                    Model = "tts-1",
                    Voice = "nova",
                });
                filePath = audioResult?.Content?.Result!;
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
        private async Task PushToWebSocket(Guid? chatBotId, string? message, string? filePath, double tokenRation, CancellationToken cancellationToken)
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

        /// <summary>
        /// Lấy số lượng token theo skill
        /// </summary>
        /// <param name="skill"></param>
        /// <param name="chatbotConfig"></param>
        /// <returns></returns>
        public static int GetSkillToken(EnumCourseSkill skill, ChatbotConfig chatbotConfig)
        {
            int token = 0;
            switch (skill)
            {
                case (EnumCourseSkill.Vocabulary):
                    token = chatbotConfig?.ChatbotTokenConfigs?.VocabularyToken ?? default;
                    break;

                case (EnumCourseSkill.Grammar):
                    token = chatbotConfig?.ChatbotTokenConfigs?.GrammarToken ?? default;
                    break;

                case (EnumCourseSkill.Listening):
                    token = chatbotConfig?.ChatbotTokenConfigs?.ListeningToken ?? default;
                    break;

                case (EnumCourseSkill.Reading):
                    token = chatbotConfig?.ChatbotTokenConfigs?.ReadingToken ?? default;
                    break;

                case (EnumCourseSkill.Writing):
                    token = chatbotConfig?.ChatbotTokenConfigs?.WritingToken ?? default;
                    break;

                case (EnumCourseSkill.Speaking):
                    token = chatbotConfig?.ChatbotTokenConfigs?.SpeakingToken ?? default;
                    break;
            }
            return token;
        }

        #endregion Func
    }
}
