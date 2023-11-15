// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Commands.QuestBoardStudentCmd
{
    using Fsel.Common.ActionResults;
    using Fsel.Shared.Enums;
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
        private readonly IQuestBoardConfigRepository _questBoardConfigRepository;

        public QuestBoardStudentCommandHandler(
            IQuestBoardRepository questBoardRepository,
            IQuestBoardStudentRepository questBoardStudentRepository,
            IQuestBoardConfigRepository questBoardConfigRepository)
        {
            _questBoardRepository = questBoardRepository;
            _questBoardStudentRepository = questBoardStudentRepository;
            _questBoardConfigRepository = questBoardConfigRepository;
        }

        public async Task<MethodResult<bool>> Handle(QuestBoardStudentCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<bool>();

            var questBoardStudent = await _questBoardStudentRepository.Queryable
                .Where(x => x.StudentId == request.StudentId && x.Status != EnumQuestBoardStudentStatus.Achieved)
                .ToListAsync(cancellationToken);

            var questBoardQuery = _questBoardRepository.Queryable
                .Where(x => request.Categories!.Contains(x.Category));

            var questBoardConfig = _questBoardConfigRepository.Queryable
                .Where(x => request.Categories!.Contains(x.Category));

            var listQuestBoard = await questBoardQuery.ToListAsync(cancellationToken);

            var questBoardStudentToUpdate = new List<QuestBoardStudent>();
            var questBoardStudentToAdd = new List<QuestBoardStudent>();

            if (request.Categories != null && request.Categories.Count > 0)
            {
                foreach (var item in request.Categories)
                {
                    QuestBoardStudent? questBoardStudentUpdate = questBoardStudent.FirstOrDefault(x => x.QuestBoard?.Category == item);
                    var questType = questBoardConfig.FirstOrDefault(x => x.Category == item)?.Type;

                    switch (questType)
                    {
                        case EnumQuestBoardType.MainQuests:
                            questBoardStudentUpdate = CustomizeDataMainQuestBoard(questBoardStudent, item, request);
                            break;
                        case EnumQuestBoardType.DailyQuests:
                            questBoardStudentUpdate = CustomizeDataDailyQuestBoard(questBoardStudent, item, request);
                            break;
                    }

                    if (questBoardStudentUpdate != null)
                    {
                        if (questBoardStudentUpdate.AchievedPoints >= request.AchievedPoints)
                        {
                            continue;
                        }

                        questBoardStudentUpdate.AchievedPoints = request.AchievedPoints;
                        questBoardStudentToUpdate.Add(questBoardStudentUpdate);
                    }
                    else
                    {
                        var questBoardAdd = listQuestBoard.FirstOrDefault(x => x.Category == item);

                        if (questBoardAdd == null)
                        {
                            continue;
                        }
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

        private static QuestBoardStudent CustomizeDataDailyQuestBoard(List<QuestBoardStudent> questBoardStudents, EnumQuestBoardCategory category, QuestBoardStudentCommand request)
        {
            var questBoardTemp = CustomizeDataMainQuestBoard(questBoardStudents, category, request);

            if (questBoardStudents != null && questBoardTemp != null && !CheckDailyQuest(questBoardTemp.CreatedDate))
            {
                questBoardTemp = null;
            }

            return questBoardTemp!;
        }

        private static QuestBoardStudent CustomizeDataMainQuestBoard(List<QuestBoardStudent> questBoardStudents, EnumQuestBoardCategory category, QuestBoardStudentCommand request)
        {
            return questBoardStudents!.FirstOrDefault(x => x.QuestBoard?.Category == category && x.StudentId == request.StudentId)!;
        }

        private static bool CheckDailyQuest(DateTime date)
        {
            var currentDate = DateTime.UtcNow;
            return date.Date == currentDate.Date && date.Month == currentDate.Month && date.Year == currentDate.Year;
        }
    }
}
