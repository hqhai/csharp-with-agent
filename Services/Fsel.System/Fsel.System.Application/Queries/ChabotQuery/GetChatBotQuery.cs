// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Queries.ChabotQuery
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.System.Domain.IRepositories;
    using Fsel.System.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetChatBotQuery : IRequest<MethodResult<ChatBotModel>>
    {
        public Guid? ChatBotId { get; set; }
    }

    public class GetChatBotQueryHandler : IRequestHandler<GetChatBotQuery, MethodResult<ChatBotModel>>
    {

        private readonly IMapper _mapper;
        private readonly IChatBotRepository _chatBotRepository;
        public GetChatBotQueryHandler(IMapper mapper, IChatBotRepository chatBotRepository)
        {
            _mapper = mapper;
            _chatBotRepository = chatBotRepository;
        }

        public async Task<MethodResult<ChatBotModel>> Handle(GetChatBotQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<ChatBotModel>();

            var chatbotMessage = await _chatBotRepository.Queryable.FirstOrDefaultAsync(x => x.Id == request.ChatBotId, cancellationToken);

            methodResult.Result = _mapper.Map<ChatBotModel>(chatbotMessage);
            methodResult.StatusCode = StatusCodes.Status201Created;
            return methodResult;
        }
    }
}
