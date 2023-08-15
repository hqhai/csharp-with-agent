// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Queries.QuestBoardQuery
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.System.Domain.IRepositories;
    using Fsel.System.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetQuestBoardsByAdminQuery : IRequest<MethodResult<IList<QuestBoardModel>>>
    {
    }

    public class GetQuestBoardsByAdminQueryHandler : IRequestHandler<GetQuestBoardsByAdminQuery, MethodResult<IList<QuestBoardModel>>>
    {
        private readonly IQuestBoardRepository _questBoardRepository;
        private readonly IMapper _mapper;

        public GetQuestBoardsByAdminQueryHandler(IQuestBoardRepository questBoardRepository, IMapper mapper)
        {
            _questBoardRepository = questBoardRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<IList<QuestBoardModel>>> Handle(GetQuestBoardsByAdminQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<IList<QuestBoardModel>>();
            var questBoards = await _questBoardRepository.Queryable.Where(x => x.IsActive && x.DependentId == null).ToListAsync(cancellationToken);
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
