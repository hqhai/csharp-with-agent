// Copyright (c) Atlantic. All rights reserved.

using AutoMapper;
using Fsel.Common.ActionResults;
using Fsel.Common.Enums.ErrorCodes;
using Fsel.System.Domain.IRepositories;
using Fsel.System.Domain.Models.EntityModels;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Fsel.System.Application.Queries.ChatbotConfigQuery
{
    public class GetUnitChatBotConfigsQuery : IRequest<MethodResult<IList<ChatbotConfigModel>>>
    {
        public IList<Guid>? UnitIds { get; set; }
    }

    public class GetUnitChatBotConfigsQueryHandler : IRequestHandler<GetUnitChatBotConfigsQuery, MethodResult<IList<ChatbotConfigModel>>>
    {
        private readonly IChatbotConfigRepository _chatbotConfigRepository;
        private readonly IMapper _mapper;

        public GetUnitChatBotConfigsQueryHandler(IChatbotConfigRepository chatbotConfigRepository, IMapper mapper)
        {
            _chatbotConfigRepository = chatbotConfigRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<IList<ChatbotConfigModel>>> Handle(GetUnitChatBotConfigsQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<IList<ChatbotConfigModel>> methodResult = new MethodResult<IList<ChatbotConfigModel>>();

            if (request.UnitIds == null || request.UnitIds.Count == 0)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(request.UnitIds));
                return methodResult;
            }

            var chatbotConfigs = await _chatbotConfigRepository.ReadQueryable
                                                               .Include(x => x.ChatbotSkillConfigs)
                                                               .Where(x => request.UnitIds.Contains(x.UnitId))
                                                               .ToListAsync(cancellationToken);

            methodResult.Result = _mapper.Map<IList<ChatbotConfigModel>>(chatbotConfigs);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
