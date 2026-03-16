// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Queries.ChabotQuery
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Helpers;
    using Fsel.System.Domain.Entities.Chatbots;
    using Fsel.System.Domain.IRepositories;
    using Fsel.System.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class GetChatBotQuery : IRequest<MethodResult<IList<ChatBotModel>>>
    {
        public Guid UnitResultId { get; set; }
        public Guid UnitId { get; set; }
        public Guid StudentId { get; set; }
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

            var chatBotConfig = await _chatBotConfigRepository.ReadQueryable.Include(x => x.ChatbotSkillConfigs)
                                                              .FirstOrDefaultAsync(x => x.UnitId == request.UnitId, cancellationToken);
            if (chatBotConfig == null)
            {
                return methodResult;
            }

            var chatbotMessage = await _chatBotRepository.ReadQueryable
                                                         .Where(x => x.UnitResultId == request.UnitResultId)
                                                         .Where(x => x.UnitId == request.UnitId && x.StudentId == request.StudentId)
                                                         .ToListAsync(cancellationToken);

            var chatbotSkillConfigs = chatBotConfig.ChatbotSkillConfigs.ToList();
            var result = _mapper.Map<IList<ChatBotModel>>(chatbotMessage);

            foreach (var item in result)
            {
                item.ChatbotLayout = chatbotSkillConfigs.FirstOrDefault(x => x.SkillId == item.SkillId)?.ChatbotLayout ?? Shared.Enums.EnumChatbotLayout.Other;
                item.ProgressRatio = Math.Round((float)item.RemainToken / GetChatBotToken(item.SkillId, chatbotSkillConfigs), ValueSettings.ChatBotSetup.RatioRound);
                item.Conversations = item.Conversations != null ? ArrayHelper.RemoveFirstTwoElements(item.Conversations, ValueSettings.ChatBotSetup.NumberDeletedElement) : null;
            }

            methodResult.Result = result;
            return methodResult;
        }

        /// <summary>
        /// </summary>
        /// <param name="skill"></param>
        /// <param name="chatbotToken"></param>
        /// <returns></returns>
        public static int GetChatBotToken(Guid? skillId, IList<ChatbotSkillConfig>? chatbotSkillConfigs)
        {
            return chatbotSkillConfigs?.FirstOrDefault(x => x.SkillId == skillId)?.Token ?? default;
        }
    }
}
