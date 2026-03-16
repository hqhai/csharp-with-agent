// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Queries.ChabotQuery
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Shared.Constants;
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

            var chatbotMessage = await _chatBotRepository.ReadQueryable.FirstOrDefaultAsync(x => x.Id == request.ChatbotId, cancellationToken);
            if (chatbotMessage == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(chatbotMessage));
                return methodResult;
            }

            var chatbotConfig = await _chatBotConfigRepository.ReadQueryable
                                                              .Include(x => x.ChatbotSkillConfigs)
                                                              .FirstOrDefaultAsync(x => x.UnitId == chatbotMessage.UnitId, cancellationToken);
            if (chatbotConfig == null)
            {
                methodResult.AddError(nameof(EnumSystemErrorCode.DataNotExist), nameof(chatbotConfig));
                return methodResult;
            }

            double tokenRatio = (float)chatbotMessage.RemainToken / GetSkillToken(chatbotMessage.SkillId, chatbotConfig);

            var chatBotModel = _mapper.Map<ChatBotModel>(chatbotMessage);
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
        public static int GetSkillToken(Guid skillId, ChatbotConfig chatbotConfig)
        {
            return chatbotConfig?.ChatbotSkillConfigs.FirstOrDefault(x => x.SkillId == skillId)?.Token ?? default;
        }
    }
}
