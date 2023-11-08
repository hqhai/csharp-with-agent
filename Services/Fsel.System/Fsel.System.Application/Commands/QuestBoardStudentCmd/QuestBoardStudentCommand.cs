// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Commands.QuestBoardStudentCmd
{
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base;
    using Fsel.Shared.Enums;
    using Fsel.System.Application.Services.UserServices;
    using Fsel.System.Domain.Entities;
    using Fsel.System.Domain.IRepositories;
    using Fsel.System.Domain.Models.CommandModels.QuestBoards;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class QuestBoardStudentCommand : CreateQuestBoardStudentModel, IRequest<MethodResult<bool>>
    {
    }

    public class QuestBoardStudentCommandHandler : IRequestHandler<QuestBoardStudentCommand, MethodResult<bool>>
    {
        private readonly IQuestBoardRepository _questBoardRepository;
        private readonly IQuestBoardStudentRepository _questBoardStudentRepository;

        public QuestBoardStudentCommandHandler(IQuestBoardRepository questBoardRepository
            , IQuestBoardStudentRepository questBoardStudentRepository)
        {
            _questBoardRepository = questBoardRepository;
            _questBoardStudentRepository = questBoardStudentRepository;
        }

        public async Task<MethodResult<bool>> Handle(QuestBoardStudentCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<bool>();

            var questBoardNotDaily = await _questBoardStudentRepository.Queryable.Where(x => x.StudentId == request.StudentId && x.Status != EnumQuestBoardStudentStatus.Achieved).ToListAsync(cancellationToken);

            var questBoardQuery = from item in request.Categories
                                  join item2 in _questBoardRepository.Queryable on item equals item2.Category
                                  select item2;

            var listQuestBoard = questBoardQuery.ToList();


            IList<QuestBoardStudent> questBoardStudentToUpdate = new List<QuestBoardStudent>();
            IList<QuestBoardStudent> questBoardStudentToAdd = new List<QuestBoardStudent>();

            //check exists questboard
            if (request.Categories != null && request.Categories.Count > 0)
            {
                foreach (var item in request.Categories)
                {
                    var questBoardNotDailyUpdate = questBoardNotDaily.FirstOrDefault(x => x.QuestBoard?.Category == item);

                    if (questBoardNotDailyUpdate != null)
                    {

                        if (questBoardNotDailyUpdate.AchievedPoints >= request.AchievedPoints)
                        {
                            continue;
                        }
                        questBoardNotDailyUpdate.AchievedPoints = request.AchievedPoints;
                        questBoardStudentToUpdate.Add(questBoardNotDailyUpdate);
                    }
                    else
                    {
                        var questBoardAdd = listQuestBoard.FirstOrDefault(x => x.Category == item);
                        var questBoardStudentAdd = new QuestBoardStudent()
                        {
                            QuestBoardId = questBoardAdd!.Id,
                            StudentId = request.StudentId,
                            AchievedPoints = request.AchievedPoints,
                            ObjectId = request.ObjectId,
                            CourseId = request.CourseId,
                        };
                        questBoardStudentToAdd.Add(questBoardStudentAdd);
                    }
                }
            }

            await _questBoardStudentRepository.ExecuteTransactionAsync(async () =>
            {
                await _questBoardStudentRepository.AddList(questBoardStudentToAdd);
                _questBoardStudentRepository.UpdateList(questBoardStudentToUpdate);
                await _questBoardStudentRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

                methodResult.StatusCode = StatusCodes.Status201Created;
                methodResult.Result = true;
                return methodResult;
            });

            return methodResult;
        }
    }
}
