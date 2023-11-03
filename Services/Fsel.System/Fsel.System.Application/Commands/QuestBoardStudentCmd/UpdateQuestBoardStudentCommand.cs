// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Commands.QuestBoardStudentCmd
{
    using Fsel.Common.ActionResults;
    using Fsel.System.Domain.IRepositories;
    using global::System;
    using global::System.Linq;
    using global::System.Threading.Tasks;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class UpdateQuestBoardStudentCommand : IRequest<MethodResult<bool>>
    {
        public Guid QuestBoardStudent { get; set; }
    }
    public class UpdateQuestBoardStudentCommandHandler : IRequestHandler<UpdateQuestBoardStudentCommand, MethodResult<bool>>
    {
        private readonly IQuestBoardStudentRepository _questBoardStudentRepository;

        public UpdateQuestBoardStudentCommandHandler(IQuestBoardStudentRepository questBoardStudentRepository)
        {
            _questBoardStudentRepository = questBoardStudentRepository;
        }

        public async Task<MethodResult<bool>> Handle(UpdateQuestBoardStudentCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<bool> methodResult = new MethodResult<bool>();
            var questBoard = await _questBoardStudentRepository.Queryable.Where(x => x.Id == request.QuestBoardStudent).ToListAsync(cancellationToken);
            var questBoards = questBoard.FirstOrDefault();
            await _questBoardStudentRepository.ExecuteTransactionAsync(async () =>
            {
                questBoards.AchievedPoints = questBoards.AchievedPoints + 1;
                questBoards = _questBoardStudentRepository.Update(questBoards);
                await _questBoardStudentRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
                methodResult.StatusCode = StatusCodes.Status201Created;
                methodResult.Result = true;
                return methodResult;
            });
            return methodResult;
        }
    }
}
