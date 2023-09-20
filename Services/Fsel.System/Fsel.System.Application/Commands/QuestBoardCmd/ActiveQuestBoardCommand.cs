// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Commands.QuestBoardCmd
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.System.Domain.IRepositories;
    using Fsel.System.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class ActiveQuestBoardCommand : IRequest<MethodResult<QuestBoardModel>>
    {
        public Guid Id { get; set; }
        public bool IsActive { get; set; }
    }

    public class ActiveQuestBoardCommandHandler : IRequestHandler<ActiveQuestBoardCommand, MethodResult<QuestBoardModel>>
    {
        private readonly IMapper _mapper;
        private readonly IQuestBoardRepository _questBoardRepository;

        public ActiveQuestBoardCommandHandler(IMapper mapper
            , IQuestBoardRepository questBoardRepository)
        {
            _mapper = mapper;
            _questBoardRepository = questBoardRepository;
        }

        public async Task<MethodResult<QuestBoardModel>> Handle(ActiveQuestBoardCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<QuestBoardModel>();
            if (!request.IsActive)
            {
                var questBoards = await _questBoardRepository.Queryable.Where(x => x.DependentId == request.Id).ToListAsync(cancellationToken);
                if (questBoards != null && questBoards.Count > 0)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataAlreadyExist), nameof(questBoards));
                    return methodResult;
                }
            }
            var questBoard = await _questBoardRepository.GetByIdAsync(request.Id);
            if (questBoard == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(questBoard));
                return methodResult;
            }
            questBoard.IsActive = request.IsActive;
            await _questBoardRepository.ExecuteTransactionAsync(async () =>
            {
                questBoard = _questBoardRepository.Update(questBoard);
                await _questBoardRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);

                methodResult.StatusCode = StatusCodes.Status200OK;
                methodResult.Result = _mapper.Map<QuestBoardModel>(questBoard);
                return methodResult;
            });

            return methodResult;
        }
    }
}
