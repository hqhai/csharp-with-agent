// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Commands.Chatbots
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Helpers;
    using Fsel.System.Domain.Entities.Chatbots;
    using Fsel.System.Domain.IRepositories;
    using Fsel.System.Domain.Models.CommandModels.ChatbotConfigs;
    using Fsel.System.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class SaveChatbotConfigCommand : SaveChatbotConfigCommandModel, IRequest<MethodResult<ChatbotConfigModel>>
    {
    }

    public class SaveChatbotConfigCommandHandler : IRequestHandler<SaveChatbotConfigCommand, MethodResult<ChatbotConfigModel>>
    {
        private readonly IChatbotConfigRepository _chatbotConfigRepository;
        private readonly AuthContext _authContext;
        private readonly IMapper _mapper;

        public SaveChatbotConfigCommandHandler(IChatbotConfigRepository chatbotConfigRepository, AuthContext authContext, IMapper mapper)
        {
            _chatbotConfigRepository = chatbotConfigRepository;
            _authContext = authContext;
            _mapper = mapper;
        }

        public async Task<MethodResult<ChatbotConfigModel>> Handle(SaveChatbotConfigCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<ChatbotConfigModel>();

            var chatbotConfig = await _chatbotConfigRepository.Queryable.Include(p => p.ChatbotSkillConfigs).FirstOrDefaultAsync(p => p.UnitId == request.UnitId, cancellationToken);

            var unitInfo = await SendMailHelper.GetTemplateFromPath(AppDomain.CurrentDomain.BaseDirectory, ResourceSettings.ChatbotUnitInfo, cancellationToken);

            if (chatbotConfig == null)
            {
                chatbotConfig = _mapper.Map<ChatbotConfig>(request);
            }
            return methodResult;
        }


    }
}
