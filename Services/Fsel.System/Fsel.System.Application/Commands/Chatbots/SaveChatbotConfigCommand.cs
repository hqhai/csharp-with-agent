// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Commands.Chatbots
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Helpers;
    using Fsel.System.Domain.Entities.Chatbots;
    using Fsel.System.Domain.IRepositories;
    using Fsel.System.Domain.Models.CommandModels.ChatbotConfigs;
    using Fsel.System.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class SaveChatbotConfigCommand : SaveChatbotConfigCommandModel, IRequest<MethodResult<ChatbotConfigModel>>
    {
    }

    public class SaveChatbotConfigCommandHandler : IRequestHandler<SaveChatbotConfigCommand, MethodResult<ChatbotConfigModel>>
    {
        private readonly IChatbotConfigRepository _chatbotConfigRepository;
        private readonly IMapper _mapper;

        public SaveChatbotConfigCommandHandler(IChatbotConfigRepository chatbotConfigRepository, IMapper mapper)
        {
            _chatbotConfigRepository = chatbotConfigRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<ChatbotConfigModel>> Handle(SaveChatbotConfigCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<ChatbotConfigModel>();

            foreach (var item in request.ChatbotSkillConfigs!)
            {
                item.AiConfig += ConvertHelper.Serialize(item.Configs);
            }

            ChatbotConfig chatbotConfig = new ChatbotConfig();
            chatbotConfig = _mapper.Map<ChatbotConfig>(request);

            await _chatbotConfigRepository.ExecuteTransactionAsync(async () =>
            {
                _chatbotConfigRepository.Add(chatbotConfig);
                await _chatbotConfigRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
                methodResult.StatusCode = StatusCodes.Status201Created;
                methodResult.Result = _mapper.Map<ChatbotConfigModel>(chatbotConfig);
                return methodResult;
            });
            return methodResult;
        }


    }
}
