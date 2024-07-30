// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Commands.Chatbots
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Helpers;
    using Fsel.System.Domain.Entities.ChatBot;
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
        public InitChatBotRoomCommandHandler(IMapper mapper, IChatBotRepository chatBotRepository, IChatbotConfigRepository chatBotConfigRepository, IMediator mediator)
        {
            _mapper = mapper;
            _chatBotRepository = chatBotRepository;
            _chatBotConfigRepository = chatBotConfigRepository;
            _mediator = mediator;
        }

        public async Task<MethodResult<ChatBotModel>> Handle(InitChatBotRoomCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<ChatBotModel>();

            var chatbotConfig = _chatBotConfigRepository.Queryable.Include(x => x.ChatbotSkillConfigs).Include(x => x.ChatbotTokenConfigs).FirstOrDefault(x => x.UnitId == request.UnitId && x.Status == EnumChatbotConfigStatus.Completed);
            var chatbotMessage = _chatBotRepository.Queryable.FirstOrDefault(x => x.UnitId == request.UnitId && x.StudentId == request.StudentId && x.Skill == request.Skill);

            string initSystemRole = ReadingSystemUserConfig(request.Skill);

            if (chatbotConfig == null)
            {
                methodResult.AddError(nameof(EnumSystemErrorCode.DataNotExist));
                return methodResult;
            }

            var chatBotSkill = chatbotConfig.ChatbotSkillConfigs.FirstOrDefault(x => x.Skill == request.Skill);

            string initUserRole = ReadingSystemUserConfig(request.Skill, chatBotSkill!.AiConfig);

            #region Exists
            if (chatbotMessage != null)
            {
                methodResult.Result = _mapper.Map<ChatBotModel>(chatbotMessage);
                methodResult.Result.Conversations = ArrayHelper.RemoveFirstTwoElements(methodResult.Result.Conversations!, ValueSettings.ChatBotSetup.NumberDeletedElement);
                return methodResult;
            }
            #endregion

            #region Init
            List<ChatBotMessageModel> initConversation = new List<ChatBotMessageModel> {
                    new ChatBotMessageModel {   Role = "system",Content =  initSystemRole},
                    new ChatBotMessageModel {   Role = "user",Content =  initUserRole},
                };

            var response = await _mediator.Send(new SubmitAICommand
            {
                MaxToken = GetSkillToken(request.Skill, chatbotConfig),
                ChatBotMessages = initConversation,
            }, cancellationToken);

            ChatBotMessageModel newMessage = new ChatBotMessageModel
            {
                Role = "system",
                Content = response
            };
            initConversation.Add(newMessage);

            ChatBot chatBot = new ChatBot();
            await _chatBotRepository.ExecuteTransactionAsync(async () =>
            {
                //Mapping
                chatBot = _mapper.Map<ChatBot>(request);
                chatBot.Conversations = _mapper.Map<List<ChatBotMessage>>(initConversation);
                chatBot.LastestAnswer = _mapper.Map<ChatBotMessage>(newMessage);
                chatBot.RemainToken = GetSkillToken(request.Skill, chatbotConfig);

                //Save
                _chatBotRepository.Add(chatBot);
                await _chatBotRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                methodResult.StatusCode = StatusCodes.Status201Created;
                methodResult.Result = _mapper.Map<ChatBotModel>(chatBot);
                methodResult.Result.Conversations = methodResult.Result.Conversations != null ? ArrayHelper.RemoveFirstTwoElements(methodResult.Result.Conversations!, ValueSettings.ChatBotSetup.NumberDeletedElement) : null;
                return methodResult;
            });
            #endregion

            methodResult.StatusCode = StatusCodes.Status201Created;
            return methodResult;
        }

        private static string ReadingSystemUserConfig(EnumCourseSkill skill, string? instruction = null)
        {
            string filePath = string.Empty;

            switch (skill)
            {
                case (EnumCourseSkill.Vocabulary):
                    filePath = instruction == null ? ResourceSettings.VocabularyRole : ResourceSettings.VocabularyInstruction;
                    break;
                case (EnumCourseSkill.Grammar):
                    filePath = instruction == null ? ResourceSettings.GrammarRole : ResourceSettings.GrammarInstruction;
                    break;
                case (EnumCourseSkill.Reading):
                    filePath = instruction == null ? ResourceSettings.ReadingRole : ResourceSettings.ReadingInstruction;
                    break;
                case (EnumCourseSkill.Speaking):
                    filePath = instruction == null ? ResourceSettings.SpeakingRole : ResourceSettings.SpeakingInstruction;
                    break;
                case (EnumCourseSkill.Writing):
                    filePath = instruction == null ? ResourceSettings.WritingRole : ResourceSettings.WritingInstruction;
                    break;
                case (EnumCourseSkill.Listening):
                    filePath = instruction == null ? ResourceSettings.ListeningRole : ResourceSettings.ListeningInstruction;
                    break;
            }
            string result = instruction == null ? File.ReadAllText(filePath) : (instruction + "\n" + File.ReadAllText(filePath));
            return result;
        }

        /// <summary>
        /// Lấy số token được config theo chatbot
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
    }
}
