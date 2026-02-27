// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Commands.Chatbots
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Helpers;
    using Fsel.System.Application.Services.CourseServices;
    using Fsel.System.Application.Services.CourseServices.Models;
    using Fsel.System.Application.Services.CourseServices.QueryModels;
    using Fsel.System.Domain.Entities.Chatbots;
    using Fsel.System.Domain.IRepositories;
    using Fsel.System.Domain.Models.CommandModels.ChatBot;
    using Fsel.System.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class InitChatBotRoomCommand : SaveChatBotMessageModel, IRequest<MethodResult<ChatBotModel>>
    {
    }

    public class InitChatBotRoomCommandHandler : IRequestHandler<InitChatBotRoomCommand, MethodResult<ChatBotModel>>
    {
        private readonly IMapper _mapper;
        private readonly IChatBotRepository _chatBotRepository;
        private readonly IChatbotConfigRepository _chatBotConfigRepository;
        private readonly IMediator _mediator;
        private readonly ICourseService _courseService;

        public InitChatBotRoomCommandHandler(
            IMapper mapper,
            IChatBotRepository chatBotRepository,
            IChatbotConfigRepository chatBotConfigRepository,
            IMediator mediator,
            ICourseService courseService)
        {
            _mapper = mapper;
            _chatBotRepository = chatBotRepository;
            _chatBotConfigRepository = chatBotConfigRepository;
            _mediator = mediator;
            _courseService = courseService;
        }

        public async Task<MethodResult<ChatBotModel>> Handle(
            InitChatBotRoomCommand request,
            CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            return await InitBySkillIdAsync(request, cancellationToken);
        }

        private async Task<MethodResult<ChatBotModel>> InitBySkillIdAsync(
            InitChatBotRoomCommand request,
            CancellationToken ct)
        {
            var result = new MethodResult<ChatBotModel>();

            var chatbotConfig = await _chatBotConfigRepository.ReadQueryable
                                                              .Include(x => x.ChatbotSkillConfigs)
                                                              .Where(x => x.UnitId == request.UnitId)
                                                              .Where(x => x.Status == EnumChatbotConfigStatus.Completed)
                                                              .FirstOrDefaultAsync(ct);
            if (chatbotConfig == null)
            {
                result.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(chatbotConfig));
                return result;
            }

            // 2️⃣ Load skill config
            var chatBotSkill = chatbotConfig.ChatbotSkillConfigs.FirstOrDefault(x => x.SkillId == request.SkillId);
            if (chatBotSkill == null)
            {
                result.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(chatBotSkill));
                return result;
            }

            // 3️⃣ Load AI criteria config
            var aiConfig = await LoadAiCriteriaConfigAsync(chatBotSkill.AICriteriaConfigId);

            // 4️⃣ Check existing chatbot
            var existingChatbot = await _chatBotRepository.ReadQueryable
                                                          .Where(x => x.UnitId == request.UnitId)
                                                          .Where(x => x.StudentId == request.StudentId)
                                                          .Where(x => x.UnitResultId == request.UnitResultId)
                                                          .FirstOrDefaultAsync(x => x.SkillId == request.SkillId, ct);
            if (existingChatbot != null)
            {
                result.Result = MapChatbot(existingChatbot);
                result.StatusCode = StatusCodes.Status200OK;
                return result;
            }

            // 5️⃣ Build initial conversation
            var initConversation = BuildInitConversation(aiConfig, chatBotSkill);
            var maxToken = chatBotSkill.Token;

            // 6️⃣ Call AI
            var aiResponse = await _mediator.Send(new SubmitAICommand
            {
                Model = aiConfig?.AiModel,
                Temperature = aiConfig?.SettingTemperature ?? default,
                PresencePenalty = aiConfig?.SettingPresence ?? default,
                TopP = aiConfig?.SettingTopP ?? default,
                MaxToken = maxToken,
                ChatBotMessages = initConversation
            }, ct);

            initConversation.Add(new ChatBotMessageModel
            {
                Role = "system",
                Content = aiResponse
            });

            return await CreateChatbotAsync(request, chatBotSkill, initConversation, maxToken, ct);
        }

        private async Task<AICriteriaConfigsModel?> LoadAiCriteriaConfigAsync(Guid? configId)
        {
            if (!configId.HasValue)
            {
                return null;
            }

            var result = await _courseService.GetConfigByIdAsync(new GetAICriteriaConfigsQueryModel
            {
                Id = configId.Value
            });

            return result?.Content?.Result;
        }

        private static IList<ChatBotMessageModel> BuildInitConversation(AICriteriaConfigsModel? aiConfig, ChatbotSkillConfig? skillConfig)
        {
            return new List<ChatBotMessageModel>
            {
                new()
                {
                    Role = "system",
                    Content = ReadingSystemUserConfig(aiConfig)
                },
                new()
                {
                    Role = "user",
                    Content = ReadingSystemUserConfig(aiConfig, skillConfig?.AiConfig)
                }
            };
        }

        private async Task<MethodResult<ChatBotModel>> CreateChatbotAsync(
            InitChatBotRoomCommand request,
            ChatbotSkillConfig chatBotSkill,
            IList<ChatBotMessageModel> conversation,
            int maxToken,
            CancellationToken ct)
        {
            MethodResult<ChatBotModel> methodResult = new MethodResult<ChatBotModel>();
            await _chatBotRepository.ExecuteTransactionAsync(async () =>
            {
                var chatBot = _mapper.Map<ChatBot>(request);
                chatBot.Conversations = _mapper.Map<List<ChatBotMessage>>(conversation);
                chatBot.LastestAnswer = _mapper.Map<ChatBotMessage>(conversation.Last());
                chatBot.RemainToken = maxToken;
                chatBot.SkillFilePath = chatBotSkill.SkillFilePath;
                chatBot.SkillName = chatBotSkill.SkillName;

                _chatBotRepository.Add(chatBot);
                await _chatBotRepository.UnitOfWork.SaveChangesAsync(ct);

                methodResult.StatusCode = StatusCodes.Status201Created;
                methodResult.Result = MapChatbot(chatBot);
                return methodResult;
            });

            return methodResult;
        }

        private ChatBotModel MapChatbot(ChatBot chatBot)
        {
            var model = _mapper.Map<ChatBotModel>(chatBot);
            model.Conversations = TrimInitMessages(model.Conversations);
            return model;
        }

        private static string ReadingSystemUserConfig(
            AICriteriaConfigsModel? aiCriteriaConfig,
            string? instruction = null)
        {
            var criteria = aiCriteriaConfig?.AiCriteriaModels?.FirstOrDefault();
            if (criteria == null)
            {
                return instruction ?? string.Empty;
            }

            var baseConfig = instruction == null ? criteria.UserRole : criteria.SettingAiConfig;

            baseConfig ??= string.Empty;
            return string.IsNullOrEmpty(instruction)
                ? baseConfig
                : $"{instruction}\n{baseConfig}";
        }

        private static IList<ChatBotMessage>? TrimInitMessages(
            IList<ChatBotMessage>? conversations)
        {
            return conversations == null
                ? null : ArrayHelper.RemoveFirstTwoElements(
                        conversations,
                        ValueSettings.ChatBotSetup.NumberDeletedElement);
        }
    }
}
