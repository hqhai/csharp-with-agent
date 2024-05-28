// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Queries.ChabotQuery
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.System.Domain.IRepositories;
    using Fsel.System.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class GetChatBotQuery : IRequest<MethodResult<IList<ChatBotModel>>>
    {
        public Guid? UnitId { get; set; }
        public Guid? StudentId { get; set; }
    }

    public class GetChatBotQueryHandler : IRequestHandler<GetChatBotQuery, MethodResult<IList<ChatBotModel>>>
    {

        private readonly IMapper _mapper;
        private readonly IChatBotRepository _chatBotRepository;
        public GetChatBotQueryHandler(IMapper mapper, IChatBotRepository chatBotRepository)
        {
            _mapper = mapper;
            _chatBotRepository = chatBotRepository;
        }

        public async Task<MethodResult<IList<ChatBotModel>>> Handle(GetChatBotQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<IList<ChatBotModel>>();

            var chatbotMessage = _chatBotRepository.Queryable.Where(x => x.UnitId == request.UnitId && x.StudentId == request.StudentId);

            methodResult.Result = _mapper.Map<IList<ChatBotModel>>(chatbotMessage);
            methodResult.StatusCode = StatusCodes.Status201Created;
            return methodResult;
        }
    }
}
