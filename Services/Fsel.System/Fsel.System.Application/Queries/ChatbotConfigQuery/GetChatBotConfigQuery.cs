// Copyright (c) Atlantic. All rights reserved.

using AutoMapper;
using Fsel.Common.ActionResults;
using Fsel.System.Domain.IRepositories;
using Fsel.System.Domain.Models.EntityModels;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Fsel.System.Application.Queries.ChatbotConfigQuery
{
    public class GetChatBotConfigQuery : IRequest<MethodResult<ChatbotConfigModel>>
    {
        public Guid UnitId { get; set; }
    }

    public class GetChatBotConfigQueryHandler : IRequestHandler<GetChatBotConfigQuery, MethodResult<ChatbotConfigModel>>
    {
        private readonly IChatbotConfigRepository _chatbotConfigRepository;
        private readonly IMapper _mapper;

        public GetChatBotConfigQueryHandler(IChatbotConfigRepository chatbotConfigRepository, IMapper mapper)
        {
            _chatbotConfigRepository = chatbotConfigRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<ChatbotConfigModel>> Handle(GetChatBotConfigQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<ChatbotConfigModel> methodResult = new MethodResult<ChatbotConfigModel>();

            ChatbotConfigModel chatbotConfigModel = new ChatbotConfigModel();

            var chatbotConfig = await _chatbotConfigRepository.ReadQueryable
                                                              .Include(x => x.ChatbotSkillConfigs)
                                                              .FirstOrDefaultAsync(x => x.UnitId == request.UnitId, cancellationToken);
            methodResult.Result = _mapper.Map<ChatbotConfigModel>(chatbotConfig);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
