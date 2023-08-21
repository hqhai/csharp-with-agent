// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Commands.QuestBoardCmd
{
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.System.Domain.IRepositories;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class DeleteQuestBoardsCommand : IRequest<MethodResult<bool>>
    {
        public IList<Guid>? Ids { get; set; }
        public Guid DependentId { get; set; }
    }

    public class DeleteQuestBoardsCommandHandler : IRequestHandler<DeleteQuestBoardsCommand, MethodResult<bool>>
    {
        private readonly IQuestBoardRepository _questBoardRepository;

        public DeleteQuestBoardsCommandHandler(IQuestBoardRepository questBoardRepository)
        {
            _questBoardRepository = questBoardRepository;
        }

        public async Task<MethodResult<bool>> Handle(DeleteQuestBoardsCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<bool>();
            if (request.Ids == null || request.Ids.Count == 0)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(request.Ids));
                return methodResult;
            }
            var questBoards = await _questBoardRepository.Queryable.Where(x => request.Ids.Contains(x.Id) && x.DependentId == request.DependentId).ToListAsync(cancellationToken);
            if (questBoards == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(questBoards));
                return methodResult;
            }
            await _questBoardRepository.ExecuteTransactionAsync(async () =>
            {
                await _questBoardRepository.DeleteListAsync(questBoards);
                await _questBoardRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);

                methodResult.StatusCode = StatusCodes.Status200OK;
                methodResult.Result = true;
                return methodResult;
            });
            return methodResult;
        }
    }
}
