// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Commands.QuestBoardStudentCmd
{
    using Fsel.Common.ActionResults;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Models.ShareModels;
    using Fsel.System.Domain.IRepositories;
    using global::System;
    using global::System.Linq;
    using global::System.Threading.Tasks;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class QuestBoardFinishOneLevelPassCommand : QuestBoardStudentQueueModel, IRequest<MethodResult<bool>>
    {
    }

    public class QuestBoardFinishOneLevelPassCommandHandler : IRequestHandler<QuestBoardFinishOneLevelPassCommand, MethodResult<bool>>
    {
        private readonly IQuestBoardRepository _questBoardRepository;
        private readonly IQuestBoardStudentRepository _questBoardStudentRepository;

        public QuestBoardFinishOneLevelPassCommandHandler(IQuestBoardRepository questBoardRepository
            , IQuestBoardStudentRepository questBoardStudentRepository)
        {
            _questBoardRepository = questBoardRepository;
            _questBoardStudentRepository = questBoardStudentRepository;
        }

        public async Task<MethodResult<bool>> Handle(QuestBoardFinishOneLevelPassCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<bool>();

            var questBoardStudent = await _questBoardStudentRepository.Queryable.Include(x => x.QuestBoard)
                               .Where(x => x.QuestBoard != null && x.QuestBoard.Category == EnumQuestBoardCategory.FinishOneLevelPass && x.Status == EnumQuestBoardStudentStatus.Process && x.StudentId == request.StudentId)
                               .FirstOrDefaultAsync(cancellationToken);
            if (questBoardStudent == null)
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

                methodResult.StatusCode = StatusCodes.Status201Created;
                methodResult.Result = true;
                return methodResult;
            });

            return methodResult;
        }
    }
}
