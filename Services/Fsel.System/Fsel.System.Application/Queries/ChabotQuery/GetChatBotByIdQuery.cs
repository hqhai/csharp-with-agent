// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Queries.ChabotQuery
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Helpers;
    using Fsel.System.Domain.Entities.Chatbots;
    using Fsel.System.Domain.IRepositories;
    using Fsel.System.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetChatBotByIdQuery : IRequest<MethodResult<ChatBotModel>>
    {
        public Guid ChatbotId { get; set; }
    }

    public class GetChatBotByIdQueryHandler : IRequestHandler<GetChatBotByIdQuery, MethodResult<ChatBotModel>>
    {
        private readonly IMapper _mapper;
        private readonly IChatBotRepository _chatBotRepository;
        private readonly IChatbotConfigRepository _chatBotConfigRepository;

        public GetChatBotByIdQueryHandler(IMapper mapper, IChatBotRepository chatBotRepository, IChatbotConfigRepository chatBotConfigRepository)
        {
            _mapper = mapper;
            _chatBotRepository = chatBotRepository;
            _chatBotConfigRepository = chatBotConfigRepository;
        }

        public async Task<MethodResult<ChatBotModel>> Handle(GetChatBotByIdQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<ChatBotModel>();

            var chatbotMessage = _chatBotRepository.Queryable.FirstOrDefault(x => x.Id == request.ChatbotId);

            if (chatbotMessage == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                return methodResult;
            }

            var chatbotConfig = _chatBotConfigRepository.Queryable.Include(x => x.ChatbotSkillConfigs).Include(x => x.ChatbotTokenConfigs).FirstOrDefault(x => x.UnitId == chatbotMessage.UnitId);

            if (chatbotConfig == null)
            {
                methodResult.AddError(nameof(EnumSystemErrorCode.DataNotExist));
                return methodResult;
            }

            double tokenRatio = (float)chatbotMessage.RemainToken / GetSkillToken(chatbotMessage.Skill, chatbotConfig);

            ChatBotModel chatBotModel = new ChatBotModel();
            chatBotModel = _mapper.Map<ChatBotModel>(chatbotMessage);
            chatBotModel.ProgressRatio = Math.Round(tokenRatio, ValueSettings.ChatBotSetup.RatioRound);

            chatBotModel.Conversations = chatBotModel.Conversations != null ? ArrayHelper.RemoveFirstTwoElements(chatBotModel.Conversations, ValueSettings.ChatBotSetup.NumberDeletedElement) : null;
            methodResult.Result = chatBotModel;
            methodResult.StatusCode = StatusCodes.Status201Created;
            return methodResult;
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
    }
}
