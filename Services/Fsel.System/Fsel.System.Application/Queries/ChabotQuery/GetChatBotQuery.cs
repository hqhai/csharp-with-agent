// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Queries.ChabotQuery
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Helpers;
    using Fsel.System.Domain.Entities.Chatbots;
    using Fsel.System.Domain.IRepositories;
    using Fsel.System.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetChatBotQuery : IRequest<MethodResult<IList<ChatBotModel>>>
    {
        public Guid? UnitId { get; set; }
        public Guid? StudentId { get; set; }
    }

    public class GetChatBotQueryHandler : IRequestHandler<GetChatBotQuery, MethodResult<IList<ChatBotModel>>>
    {

        private readonly IMapper _mapper;
        private readonly IChatBotRepository _chatBotRepository;
        private readonly IChatbotConfigRepository _chatBotConfigRepository;
        public GetChatBotQueryHandler(IMapper mapper, IChatBotRepository chatBotRepository, IChatbotConfigRepository chatBotConfigRepository)
        {
            _mapper = mapper;
            _chatBotRepository = chatBotRepository;
            _chatBotConfigRepository = chatBotConfigRepository;
        }

        public async Task<MethodResult<IList<ChatBotModel>>> Handle(GetChatBotQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<IList<ChatBotModel>>();

            var chatbotMessage = _chatBotRepository.Queryable.Where(x => x.UnitId == request.UnitId && x.StudentId == request.StudentId);

            var chatBotTokenConfigs = _chatBotConfigRepository.Queryable.Include(x => x.ChatbotTokenConfigs).FirstOrDefault(x => x.UnitId == request.UnitId)?.ChatbotTokenConfigs;

            var result = _mapper.Map<IList<ChatBotModel>>(chatbotMessage);

            foreach (var item in result)
            {

                item.ProgressRatio = Math.Round((float)item.RemainToken / GetChatBotToken(item.Skill, chatBotTokenConfigs), ValueSettings.ChatBotSetup.RatioRound);
                item.Conversations = item.Conversations != null ? ArrayHelper.RemoveFirstTwoElements(item.Conversations, ValueSettings.ChatBotSetup.NumberDeletedElement) : null;
            }

            methodResult.Result = result;
            methodResult.StatusCode = StatusCodes.Status201Created;
            return methodResult;
        }


        /// <summary>
        /// </summary>
        /// <param name="skill"></param>
        /// <param name="chatbotToken"></param>
        /// <returns></returns>
        public static int GetChatBotToken(EnumCourseSkill skill, ChatbotTokenConfigs? chatbotToken)
        {
            int result = 0;
            switch (skill)
            {
                case EnumCourseSkill.Vocabulary:
                    result = chatbotToken?.VocabularyToken ?? 0;
                    break;
                case EnumCourseSkill.Grammar:
                    result = chatbotToken?.GrammarToken ?? 0;
                    break;
                case EnumCourseSkill.Listening:
                    result = chatbotToken?.ListeningToken ?? 0;
                    break;
                case EnumCourseSkill.Writing:
                    result = chatbotToken?.WritingToken ?? 0;
                    break;
                case EnumCourseSkill.Reading:
                    result = chatbotToken?.ReadingToken ?? 0;
                    break;
                case EnumCourseSkill.Speaking:
                    result = chatbotToken?.SpeakingToken ?? 0;
                    break;
            }
            return result;
        }

    }
}
