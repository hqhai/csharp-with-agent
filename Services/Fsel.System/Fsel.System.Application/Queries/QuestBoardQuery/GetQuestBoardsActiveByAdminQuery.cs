// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Queries.QuestBoardQuery
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Shared.Enums;
    using Fsel.System.Domain.IRepositories;
    using Fsel.System.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetQuestBoardsActiveByAdminQuery : IRequest<MethodResult<IList<QuestBoardModel>>>
    {
        public EnumQuestBoardType? QuestBoardType { get; set; }
    }

    public class GetQuestBoardsActiveByAdminQueryHandler : IRequestHandler<GetQuestBoardsActiveByAdminQuery, MethodResult<IList<QuestBoardModel>>>
    {
        private readonly IQuestBoardRepository _questBoardRepository;
        private readonly IMapper _mapper;

        public GetQuestBoardsActiveByAdminQueryHandler(IQuestBoardRepository questBoardRepository, IMapper mapper)
        {
            _questBoardRepository = questBoardRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<IList<QuestBoardModel>>> Handle(GetQuestBoardsActiveByAdminQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<IList<QuestBoardModel>>();

            if (request.QuestBoardType == null)
            {
                methodResult.Result = null;
                methodResult.StatusCode = StatusCodes.Status200OK;
                return methodResult;
            }

            var questBoards = await _questBoardRepository.Queryable.Where(x => x.Type == request.QuestBoardType && x.IsActive && x.DependentId == null).ToListAsync(cancellationToken);
            if (questBoards == null || questBoards.Count == 0)
            {
                methodResult.Result = null;
                methodResult.StatusCode = StatusCodes.Status200OK;
                return methodResult;
            }
            methodResult.Result = _mapper.Map<IList<QuestBoardModel>>(questBoards);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
