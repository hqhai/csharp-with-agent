// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Commands.QuestBoardCmd
{
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.System.Domain.IRepositories;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class DeleteQuestBoardCommand : IRequest<MethodResult<bool>>
    {
        public Guid Id { get; set; }
    }

    public class DeleteQuestBoardCommandHandler : IRequestHandler<DeleteQuestBoardCommand, MethodResult<bool>>
    {
        private readonly IQuestBoardRepository _questBoardRepository;

        public DeleteQuestBoardCommandHandler(IQuestBoardRepository questBoardRepository)
        {
            _questBoardRepository = questBoardRepository;
        }

        public async Task<MethodResult<bool>> Handle(DeleteQuestBoardCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<bool>();
            var questBoard = await _questBoardRepository.Queryable.Include(x => x.QuestBoardTasks).ThenInclude(x => x.QuestBoardTaskStudents).FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
            if (questBoard == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(questBoard));
                return methodResult;
            }

            await _questBoardRepository.ExecuteTransactionAsync(async () =>
            {
                await _questBoardRepository.DeleteAsync(questBoard);
                await _questBoardRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);

                methodResult.StatusCode = StatusCodes.Status200OK;
                methodResult.Result = true;
                return methodResult;
            });
            return methodResult;
        }
    }
}
