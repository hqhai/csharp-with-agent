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
        private readonly IQuestBoardOverallRepository _questBoardOverallRepository;
        private readonly IQuestBoardOverallStudentRepository _questBoardOverallStudentRepository;

        public DoQuestBoardCommandHandler(IQuestBoardRepository questBoardRepository, IQuestBoardStudentRepository questBoardStudentRepository, IQuestBoardOverallRepository questBoardOverallRepository, IQuestBoardOverallStudentRepository questBoardOverallStudentRepository)
        {
            _questBoardRepository = questBoardRepository;
            _questBoardStudentRepository = questBoardStudentRepository;
            _questBoardOverallRepository = questBoardOverallRepository;
            _questBoardOverallStudentRepository = questBoardOverallStudentRepository;
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
            if (questBoard != null)
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

                        var questBoards = await _questBoardRepository.Queryable.Where(p => p.Type == request.Type).ToListAsync(cancellationToken);
                        var questBoardIds = questBoards.Select(p => p.Id).ToList();
                        var questBoardStudents = await _questBoardStudentRepository.Queryable.Where(p => p.StudentId == request.StudentID && questBoardIds.Contains(p.QuestBoardId)).ToListAsync(cancellationToken);

                        if (questBoardStudents.Count <= 2)
                        {
                            await DoQuestBoardOverallBeginnerQuests(request, questBoardStudents, 2, cancellationToken);
                        }
                        else if (questBoardStudents.Count <= 4)
                        {
                            await DoQuestBoardOverallBeginnerQuests(request, questBoardStudents, 4, cancellationToken);
                        }
                        else if (questBoardStudents.Count <= 6)
                        {
                            await DoQuestBoardOverallBeginnerQuests(request, questBoardStudents, 6, cancellationToken);
                        }

                        return methodResult;
                    });
                }
            }
        }

        private async Task DoQuestBoardOverallBeginnerQuests(DoQuestBoardCommandModel request, List<QuestBoardStudent>? questBoardStudents, int targetValue, CancellationToken cancellationToken)
        {
            var questBoardOverall = await _questBoardOverallRepository.Queryable.FirstOrDefaultAsync(p => p.Type == EnumQuestBoardType.BeginnerQuests && p.TargetValue == targetValue, cancellationToken);
            if (questBoardOverall != null)
            {
                var questBoardOverallStudent = await _questBoardOverallStudentRepository.Queryable.FirstOrDefaultAsync(p => p.StudentId == request.StudentID && p.QuestBoardOverallId == questBoardOverall.Id, cancellationToken);
                if (questBoardOverallStudent == null)
                {
                    _questBoardOverallStudentRepository.Add(new QuestBoardOverallStudent()
                    {
                        QuestBoardOverallId = questBoardOverall.Id,
                        StudentId = request.StudentID,
                        CurrentValue = questBoardStudents == null ? 0 : questBoardStudents.Count,
                        Token = questBoardOverall.Token,
                        Status = EnumQuestBoardOverallStudentStatus.NotReceived
                    });
                }
                else
                {
                    questBoardOverallStudent.CurrentValue = questBoardStudents == null ? 0 : questBoardStudents.Count;
                }
                await _questBoardOverallStudentRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
            }
        }

        private async Task DoLearningQuests(DoQuestBoardCommandModel request, MethodResult<VoidMethodResult> methodResult, CancellationToken cancellationToken)
        {
            var questBoards = await _questBoardRepository.Queryable.Where(p => p.Type == request.Type).ToListAsync(cancellationToken);
            var questBoard = questBoards.FirstOrDefault(p => p.Category == request.Category);

            var days = Shared.Helpers.DateTimeHelper.GetWeekDays(DateTime.UtcNow).OrderBy(p => p).ToList();
            var monDay = days.First();
            var sunDay = days.Last();

            if (questBoard != null)
            {
                var learningQuest = new QuestBoardStudent();
                if (questBoard.RepeatType == EnumRepeatType.Day)
                {
                    learningQuest = await _questBoardStudentRepository.Queryable.FirstOrDefaultAsync(p => p.StudentId == request.StudentID && p.QuestBoardId == questBoard.Id && p.CreatedDate.Date == DateTime.UtcNow.Date, cancellationToken);
                }
                else
                {
                    learningQuest = await _questBoardStudentRepository.Queryable.FirstOrDefaultAsync(p => p.StudentId == request.StudentID && p.QuestBoardId == questBoard.Id && p.CreatedDate.Date >= monDay.Date && p.CreatedDate.Date <= sunDay.Date, cancellationToken);
                }

                var questBoardIds = questBoards.Select(p => p.Id).ToList();

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

                        await DoQuestBoardOverallLearningQuests(request, questBoardIds, monDay, sunDay, cancellationToken);

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

                        await DoQuestBoardOverallLearningQuests(request, questBoardIds, monDay, sunDay, cancellationToken);

                        return methodResult;
                    });
                }
            }
        }

        private async Task DoQuestBoardOverallLearningQuests(DoQuestBoardCommandModel request, List<Guid> questBoardIds, DateTime monDay, DateTime sunDay, CancellationToken cancellationToken)
        {
            var questBoardOverall = await _questBoardOverallRepository.Queryable.FirstOrDefaultAsync(p => p.Type == EnumQuestBoardType.LearningQuests, cancellationToken);
            if (questBoardOverall != null)
            {
                var questBoardOverallStudent = await _questBoardOverallStudentRepository.Queryable.FirstOrDefaultAsync(p => p.StudentId == request.StudentID && p.QuestBoardOverallId == questBoardOverall.Id && p.CreatedDate.Date >= monDay.Date && p.CreatedDate.Date <= sunDay.Date, cancellationToken);

                var questBoardStudents = await _questBoardStudentRepository.Queryable.Where(p => questBoardIds.Contains(p.QuestBoardId) && p.StudentId == request.StudentID && p.Status == EnumQuestBoardStudentStatus.Received).ToListAsync(cancellationToken);

                var totalEnergy = questBoardStudents.Sum(p => p.Energy);

                if (questBoardOverallStudent == null)
                {
                    _questBoardOverallStudentRepository.Add(new QuestBoardOverallStudent()
                    {
                        QuestBoardOverallId = questBoardOverall.Id,
                        StudentId = request.StudentID,
                        CurrentValue = totalEnergy ?? 0,
                        Token = questBoardOverall.Token,
                        Status = EnumQuestBoardOverallStudentStatus.NotReceived
                    });
                }
                else
                {
                    questBoardOverallStudent.CurrentValue = totalEnergy ?? 0;
                }
                await _questBoardOverallStudentRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
            }
        }
    }
}
