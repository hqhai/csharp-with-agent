// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Queries.QuestBoardQuery
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.System.Domain.IRepositories;
    using Fsel.System.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetQuestBoardByDependentIdQuery : IRequest<MethodResult<IList<QuestBoardModel>>>
    {
        public Guid DependentId { get; set; }
    }

    public class GetQuestBoardByDependentIdQueryHandler : IRequestHandler<GetQuestBoardByDependentIdQuery, MethodResult<IList<QuestBoardModel>>>
    {
        private readonly IQuestBoardRepository _questBoardRepository;
        private readonly IMapper _mapper;

        public GetQuestBoardByDependentIdQueryHandler(IQuestBoardRepository questBoardRepository, IMapper mapper)
        {
            _questBoardRepository = questBoardRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<IList<QuestBoardModel>>> Handle(GetQuestBoardByDependentIdQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<IList<QuestBoardModel>>();
            var questBoard = await _questBoardRepository.GetByIdAsync(request.DependentId);
            if (questBoard == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(questBoard));
                return methodResult;
            }
            var questBoards = await _questBoardRepository.Queryable.Where(x => x.DependentId == request.DependentId).ToListAsync(cancellationToken);
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
