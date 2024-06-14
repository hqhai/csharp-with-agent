// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Commands.QuestBoardCmd
{
    using Fsel.Common.ActionResults;
    using Fsel.Shared.Enums;
    using Fsel.System.Domain.Entities.QuestBoards;
    using Fsel.System.Domain.IRepositories;
    using Fsel.System.Domain.Models.CommandModels.QuestBoards;
    using global::System.Threading;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class DoQuestBoardCommand : DoQuestBoardCommandModel, IRequest<MethodResult<VoidMethodResult>>
    {
    }

    public class DoQuestBoardCommandHandler : IRequestHandler<DoQuestBoardCommand, MethodResult<VoidMethodResult>>
    {
        private readonly IQuestBoardRepository _questBoardRepository;
        private readonly IQuestBoardStudentRepository _questBoardStudentRepository;

        public DoQuestBoardCommandHandler(IQuestBoardRepository questBoardRepository, IQuestBoardStudentRepository questBoardStudentRepository)
        {
            _questBoardRepository = questBoardRepository;
            _questBoardStudentRepository = questBoardStudentRepository;
        }

        public async Task<MethodResult<VoidMethodResult>> Handle(DoQuestBoardCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<VoidMethodResult>();
            if (request.Type == EnumQuestBoardType.BeginnerQuests)
            {
                await DoBeginnerQuests(request, methodResult, cancellationToken);
            }
            else
            {
                await DoLearningQuests(request, methodResult, cancellationToken);
            }
            return methodResult;
        }

        private async Task DoBeginnerQuests(DoQuestBoardCommandModel request, MethodResult<VoidMethodResult> methodResult, CancellationToken cancellationToken)
        {
            var questBoard = await _questBoardRepository.Queryable.FirstOrDefaultAsync(p => p.Type == request.Type && p.Category == request.Category, cancellationToken);
            if (questBoard != null && questBoard.IsActive)
            {
                var questBoardStudent = await _questBoardStudentRepository.Queryable.FirstOrDefaultAsync(p => p.StudentId == request.StudentID && p.QuestBoardId == questBoard.Id, cancellationToken);
                if (questBoardStudent == null)
                {
                    await _questBoardStudentRepository.ExecuteTransactionAsync(async () =>
                    {
                        _questBoardStudentRepository.Add(new QuestBoardStudent()
                        {
                            QuestBoardId = questBoard.Id,
                            StudentId = request.StudentID,
                            CurrentValue = 1,
                            Token = questBoard.Token,
                            Status = EnumQuestBoardStudentStatus.NotReceived
                        });
                        await _questBoardStudentRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
                        return methodResult;
                    });
                }
            }
        }

        private async Task DoLearningQuests(DoQuestBoardCommandModel request, MethodResult<VoidMethodResult> methodResult, CancellationToken cancellationToken)
        {
            var questBoards = await _questBoardRepository.Queryable.Where(p => p.Type == request.Type).ToListAsync(cancellationToken);
            var questBoard = questBoards.FirstOrDefault(p => p.Category == request.Category);

            var weekDays = Shared.Helpers.DateTimeHelper.GetWeekDays(DateTime.UtcNow).OrderBy(p => p).ToList();
            var monDay = weekDays.First();
            var sunDay = weekDays.Last();

            if (questBoard != null && questBoard.IsActive)
            {
                var learningQuest = new QuestBoardStudent();
                if (questBoard.RepeatType == EnumRepeatType.Daily)
                {
                    learningQuest = await _questBoardStudentRepository.Queryable.FirstOrDefaultAsync(p => p.StudentId == request.StudentID && p.QuestBoardId == questBoard.Id && p.CreatedDate.Date == DateTime.UtcNow.Date, cancellationToken);
                }
                else
                {
                    learningQuest = await _questBoardStudentRepository.Queryable.FirstOrDefaultAsync(p => p.StudentId == request.StudentID && p.QuestBoardId == questBoard.Id && p.CreatedDate.Date >= monDay.Date && p.CreatedDate.Date <= sunDay.Date, cancellationToken);
                }

                if (learningQuest == null)
                {
                    await _questBoardStudentRepository.ExecuteTransactionAsync(async () =>
                    {
                        _questBoardStudentRepository.Add(new QuestBoardStudent()
                        {
                            QuestBoardId = questBoard.Id,
                            StudentId = request.StudentID,
                            CurrentValue = request.Value,
                            Token = questBoard.Token,
                            Energy = questBoard.Energy,
                            Status = EnumQuestBoardStudentStatus.NotReceived
                        });
                        await _questBoardStudentRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
                        return methodResult;
                    });
                }
                else
                {
                    await _questBoardStudentRepository.ExecuteTransactionAsync(async () =>
                    {
                        learningQuest.CurrentValue += request.Value;
                        learningQuest.CurrentValue = learningQuest.CurrentValue > questBoard.TargetValue ? questBoard.TargetValue : learningQuest.CurrentValue;
                        await _questBoardStudentRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
                        return methodResult;
                    });
                }
            }
        }
    }
}
