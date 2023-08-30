// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Commands.QuestBoardStudentCmd
{
    using Fsel.Common.ActionResults;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Models.ShareModels;
    using Fsel.System.Domain.IRepositories;
    using global::System;
    using global::System.Threading.Tasks;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class QuestBoardMainQuestsCommand : QuestBoardStudentQueueModel, IRequest<MethodResult<bool>>
    {
    }

    public class UpdateQuestBoardStudentCommandHandler : IRequestHandler<QuestBoardMainQuestsCommand, MethodResult<bool>>
    {
        private readonly IQuestBoardRepository _questBoardRepository;
        private readonly IQuestBoardStudentRepository _questBoardStudentRepository;

        public UpdateQuestBoardStudentCommandHandler(IQuestBoardRepository questBoardRepository
            , IQuestBoardStudentRepository questBoardStudentRepository)
        {
            _questBoardRepository = questBoardRepository;
            _questBoardStudentRepository = questBoardStudentRepository;
        }

        public async Task<MethodResult<bool>> Handle(QuestBoardMainQuestsCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<bool>();

            var questBoardStudent = await _questBoardStudentRepository.Queryable.Include(x => x.QuestBoard)
                                .Where(x => x.QuestBoard != null && x.QuestBoard.Category == request.QuestBoardCategory && x.QuestBoard.Type == request.QuestBoardType)
                                .FirstOrDefaultAsync(x => x.Status == EnumQuestBoardStudentStatus.Process && x.StudentId == request.StudentId, cancellationToken);
            var questBoard = questBoardStudent?.QuestBoard;
            var date = DateTime.Now;
            if (questBoardStudent == null || (questBoard != null && (questBoard.StartDate >= date || questBoard.EndDate < date)))
            {
                methodResult.Result = false;
                methodResult.StatusCode = StatusCodes.Status200OK;
                return methodResult;
            }
            questBoardStudent.Status = EnumQuestBoardStudentStatus.Done;
            await _questBoardStudentRepository.ExecuteTransactionAsync(async () =>
            {
                questBoardStudent = _questBoardStudentRepository.Update(questBoardStudent);
                await _questBoardRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
                methodResult.StatusCode = StatusCodes.Status200OK;
                methodResult.Result = true;
                return methodResult;
            });

            return methodResult;
        }
    }
}
