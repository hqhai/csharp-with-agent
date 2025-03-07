// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Commands.QuestBoardCmd
{
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base;
    using Fsel.Core.Extensions;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Models.ShareModels;
    using Fsel.System.Application.Commands.TokenHistoryCmd;
    using Fsel.System.Application.Services.UserServices;
    using Fsel.System.Domain.Entities.QuestBoards;
    using Fsel.System.Domain.Enums.ErrorCodes;
    using Fsel.System.Domain.IRepositories;
    using Fsel.System.Domain.Models.CommandModels.QuestBoards;
    using global::System;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class ReceiveTokenFromQuestBoardDoneCommand : ReceiveTokenFromQuestBoardDoneCommandModel, IRequest<MethodResult<VoidMethodResult>>
    {
    }

    public class ReceiveTokenFromQuestBoardDoneCommandHandler : IRequestHandler<ReceiveTokenFromQuestBoardDoneCommand, MethodResult<VoidMethodResult>>
    {
        private readonly IQuestBoardOverallRepository _questBoardOverallRepository;
        private readonly IQuestBoardOverallStudentRepository _questBoardOverallStudentRepository;
        private readonly AuthContext _authContext;
        private readonly IQuestBoardRepository _questBoardRepository;
        private readonly IQuestBoardStudentRepository _questBoardStudentRepository;
        private readonly IMediator _mediator;
        private readonly IUserService _userService;
        private const int TargetValue = 140;

        public ReceiveTokenFromQuestBoardDoneCommandHandler(IQuestBoardOverallRepository questBoardOverallRepository, IQuestBoardOverallStudentRepository questBoardOverallStudentRepository, AuthContext authContext, IQuestBoardRepository questBoardRepository, IQuestBoardStudentRepository questBoardStudentRepository, IMediator mediator, IUserService userService)
        {
            _questBoardOverallRepository = questBoardOverallRepository;
            _questBoardOverallStudentRepository = questBoardOverallStudentRepository;
            _authContext = authContext;
            _questBoardRepository = questBoardRepository;
            _questBoardStudentRepository = questBoardStudentRepository;
            _mediator = mediator;
            _userService = userService;
        }

        public async Task<MethodResult<VoidMethodResult>> Handle(ReceiveTokenFromQuestBoardDoneCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<VoidMethodResult>();

            if ((request.IsQuestBoardOverall && !request.QuestBoardOverallId.HasValue) || (!request.IsQuestBoardOverall && !request.QuestBoardId.HasValue))
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                return methodResult;
            }

            var studentResult = await _userService.GetStudentByUserIdAsync(_authContext.CurrentUserId);
            if (!studentResult.IsSuccessStatusCode)
            {
                methodResult.AddError(studentResult.Error);
                return methodResult;
            }
            var student = studentResult.Content?.Result;

            var weekDays = Shared.Helpers.DateTimeHelper.GetWeekDays(DateTime.UtcNow).OrderBy(p => p).ToList();
            var monDay = weekDays.First();
            var sunDay = weekDays.Last();

            if (request.IsQuestBoardOverall)
            {
                await ReceiveTokenFromQuestBoardOverall(request, student!.Id, monDay, sunDay, methodResult, cancellationToken);
            }
            else
            {
                await ReceiveTokenFromQuestBoard(request, student!.Id, monDay, sunDay, methodResult, cancellationToken);
            }

            return methodResult;
        }

        private async Task ReceiveTokenFromQuestBoardOverall(ReceiveTokenFromQuestBoardDoneCommandModel request, Guid studentId, DateTime monDay, DateTime sunDay, MethodResult<VoidMethodResult> methodResult, CancellationToken cancellationToken)
        {
            var questBoardOverall = await _questBoardOverallRepository.Queryable.FirstOrDefaultAsync(p => p.Id == request.QuestBoardOverallId, cancellationToken);

            if (questBoardOverall == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                return;
            }

            var questBoardOverallStudent = new QuestBoardOverallStudent();

            if (questBoardOverall.Type == EnumQuestBoardType.BeginnerQuests)
            {
                questBoardOverallStudent = await _questBoardOverallStudentRepository.Queryable.Where(p => p.QuestBoardOverallId == questBoardOverall.Id && p.StudentId == studentId).OrderBy(p => p.CreatedDate).FirstOrDefaultAsync(cancellationToken);
            }
            else
            {
                questBoardOverallStudent = await _questBoardOverallStudentRepository.Queryable.Where(p => p.QuestBoardOverallId == questBoardOverall.Id && p.StudentId == studentId && p.CurrentValue >= TargetValue && p.Status == EnumQuestBoardOverallStudentStatus.NotReceived).OrderBy(p => p.CreatedDate).FirstOrDefaultAsync(cancellationToken);
            }

            if (questBoardOverallStudent == null || questBoardOverall.TargetValue > questBoardOverallStudent.CurrentValue)
            {
                methodResult.AddErrorBadRequest(nameof(EnumQuestBoardErrorCode.HaveNotCompletedTheTask));
                return;
            }
            if (questBoardOverallStudent.Status == EnumQuestBoardOverallStudentStatus.Received)
            {
                methodResult.AddErrorBadRequest(nameof(EnumQuestBoardErrorCode.TokensHaveBeenReceived));
                return;
            }

            await _questBoardOverallStudentRepository.ExecuteTransactionAsync(async () =>
            {
                var updateTokenStudent = await _mediator.Send(new CreateTokenHistoryCommand()
                {
                    TokenHistorys = new List<TokenHistoryQueueModel>()
                    {
                        new TokenHistoryQueueModel()
                        {
                            VolatileToken = questBoardOverall.Token,
                            Feature = EnumTokenFeature.QuestBoard,
                            Mission = GetEnumTokenMission(questBoardOverall),
                            UserId = _authContext.CurrentUserId,
                            Type = EnumTokenHistoryType.Recevived,
                        }
                    }
                });
                if (!updateTokenStudent.IsOK)
                {
                    methodResult.AddError(updateTokenStudent.ErrorMessages);
                }
                questBoardOverallStudent.Status = EnumQuestBoardOverallStudentStatus.Received;
                await _questBoardOverallStudentRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                return methodResult;
            });
        }

        private static EnumTokenMission? GetEnumTokenMission(QuestBoardOverall questBoardOverall)
        {
            if (questBoardOverall.Type == EnumQuestBoardType.LearningQuests)
            {
                return EnumTokenMission.CompleteWeeklyTasks;
            }
            else if (questBoardOverall.Type == EnumQuestBoardType.BeginnerQuests && questBoardOverall.TargetValue == 2)
            {
                return EnumTokenMission.CompleteTwoBeginnerMissions;
            }
            else if (questBoardOverall.Type == EnumQuestBoardType.BeginnerQuests && questBoardOverall.TargetValue == 4)
            {
                return EnumTokenMission.CompleteFourBeginnerMissions;
            }
            else if (questBoardOverall.Type == EnumQuestBoardType.BeginnerQuests && questBoardOverall.TargetValue == 6)
            {
                return EnumTokenMission.CompleteSixBeginnerMissions;
            }
            return null;
        }

        private async Task ReceiveTokenFromQuestBoard(ReceiveTokenFromQuestBoardDoneCommandModel request, Guid studentId, DateTime monDay, DateTime sunDay, MethodResult<VoidMethodResult> methodResult, CancellationToken cancellationToken)
        {
            var questBoard = await _questBoardRepository.Queryable.FirstOrDefaultAsync(p => p.Id == request.QuestBoardId, cancellationToken);

            if (questBoard == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                return;
            }

            var questBoardStudent = new QuestBoardStudent();

            if (questBoard.Type == EnumQuestBoardType.BeginnerQuests)
            {
                questBoardStudent = await _questBoardStudentRepository.Queryable.Where(p => p.QuestBoardId == questBoard.Id && p.StudentId == studentId).OrderBy(p => p.CreatedDate).FirstOrDefaultAsync(cancellationToken);
            }
            else
            {
                if (questBoard.RepeatType == EnumRepeatType.Daily)
                {
                    questBoardStudent = await _questBoardStudentRepository.Queryable.Where(p => p.QuestBoardId == questBoard.Id && p.StudentId == studentId && p.CreatedDate.Date == DateTime.UtcNow.Date).OrderBy(p => p.CreatedDate).FirstOrDefaultAsync(cancellationToken);
                }
                else
                {
                    questBoardStudent = await _questBoardStudentRepository.Queryable.Where(p => p.QuestBoardId == questBoard.Id && p.StudentId == studentId && p.CreatedDate.Date >= monDay.Date && p.CreatedDate.Date <= sunDay.Date).OrderBy(p => p.CreatedDate).FirstOrDefaultAsync(cancellationToken);
                }
            }

            if (questBoardStudent == null || questBoard.TargetValue > questBoardStudent.CurrentValue)
            {
                methodResult.AddErrorBadRequest(nameof(EnumQuestBoardErrorCode.HaveNotCompletedTheTask));
                return;
            }

            if (questBoardStudent.Status == EnumQuestBoardStudentStatus.Received)
            {
                methodResult.AddErrorBadRequest(nameof(EnumQuestBoardErrorCode.TokensHaveBeenReceived));
                return;
            }

            await _questBoardStudentRepository.ExecuteTransactionAsync(async () =>
            {
                var updateTokenStudent = await _mediator.Send(new CreateTokenHistoryCommand()
                {
                    TokenHistorys = new List<TokenHistoryQueueModel>()
                    {
                        new TokenHistoryQueueModel()
                        {
                            VolatileToken = questBoard.Token,
                            Feature = EnumTokenFeature.QuestBoard,
                            Mission = CheckEnum(questBoard.Category),
                            UserId = _authContext.CurrentUserId,
                            Type = EnumTokenHistoryType.Recevived,
                        }
                    }
                });
                if (!updateTokenStudent.IsOK)
                {
                    methodResult.AddError(updateTokenStudent.ErrorMessages);
                }

                questBoardStudent.Status = EnumQuestBoardStudentStatus.Received;
                await _questBoardOverallStudentRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

                if (questBoard.Type == EnumQuestBoardType.BeginnerQuests)
                {
                    var questBoards = await _questBoardRepository.Queryable.Where(p => p.Type == EnumQuestBoardType.BeginnerQuests).ToListAsync(cancellationToken);
                    var questBoardOveralls = await _questBoardOverallRepository.Queryable.Where(p => p.Type == EnumQuestBoardType.BeginnerQuests).ToListAsync(cancellationToken);
                    var maxValue = questBoardOveralls.Max(p => p.TargetValue);
                    var questBoardIds = questBoards.Select(p => p.Id).ToList();
                    var questBoardStudents = await _questBoardStudentRepository.Queryable.Where(p => p.StudentId == studentId && questBoardIds.Contains(p.QuestBoardId) && p.Status == EnumQuestBoardStudentStatus.Received).ToListAsync(cancellationToken);

                    if (questBoardStudents.Count <= 2)
                    {
                        await DoQuestBoardOverallBeginnerQuests(studentId, 2, cancellationToken);
                    }
                    else if (questBoardStudents.Count <= 4)
                    {
                        await DoQuestBoardOverallBeginnerQuests(studentId, 4, cancellationToken);
                    }
                    else if (questBoardStudents.Count <= maxValue)
                    {
                        await DoQuestBoardOverallBeginnerQuests(studentId, maxValue, cancellationToken);
                    }
                }
                else
                {
                    await DoQuestBoardOverallLearningQuests(studentId, questBoardStudent.Energy, monDay, sunDay, cancellationToken);
                }

                return methodResult;
            });
        }

        public static EnumTokenMission? CheckEnum(EnumQuestBoardCategory value)
        {
            // Lấy tất cả các giá trị của EnumB
            var enumBValues = Enum.GetValues(typeof(EnumTokenMission));

            // Duyệt qua các giá trị của EnumB và so sánh
            foreach (EnumTokenMission enumBValue in enumBValues)
            {
                if (enumBValue.ToString() == value.ToString())
                {
                    return enumBValue;
                }
            }

            // Nếu không tìm thấy, trả về null
            return null;
        }

        private async Task DoQuestBoardOverallBeginnerQuests(Guid studentId, int targetValue, CancellationToken cancellationToken)
        {
            var questBoardOverall = await _questBoardOverallRepository.Queryable.FirstOrDefaultAsync(p => p.Type == EnumQuestBoardType.BeginnerQuests && p.TargetValue == targetValue, cancellationToken);
            if (questBoardOverall != null)
            {
                var questBoardOverallStudent = await _questBoardOverallStudentRepository.Queryable.FirstOrDefaultAsync(p => p.StudentId == studentId && p.QuestBoardOverallId == questBoardOverall.Id, cancellationToken);
                if (questBoardOverallStudent == null)
                {
                    _questBoardOverallStudentRepository.Add(new QuestBoardOverallStudent()
                    {
                        QuestBoardOverallId = questBoardOverall.Id,
                        StudentId = studentId,
                        CurrentValue = targetValue == 7 ? targetValue - 2 : targetValue - 1,
                        Token = questBoardOverall.Token,
                        Status = EnumQuestBoardOverallStudentStatus.NotReceived
                    });
                }
                else
                {
                    questBoardOverallStudent.CurrentValue += 1;
                }
                await _questBoardOverallStudentRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
            }
        }

        private async Task DoQuestBoardOverallLearningQuests(Guid studentId, int? energy, DateTime monDay, DateTime sunDay, CancellationToken cancellationToken)
        {
            var questBoardOverall = await _questBoardOverallRepository.Queryable.FirstOrDefaultAsync(p => p.Type == EnumQuestBoardType.LearningQuests, cancellationToken);
            if (questBoardOverall != null)
            {
                var questBoardOverallStudent = await _questBoardOverallStudentRepository.Queryable.FirstOrDefaultAsync(p => p.StudentId == studentId && p.QuestBoardOverallId == questBoardOverall.Id && p.CreatedDate.Date >= monDay.Date && p.CreatedDate.Date <= sunDay.Date, cancellationToken);

                if (questBoardOverallStudent == null)
                {
                    _questBoardOverallStudentRepository.Add(new QuestBoardOverallStudent()
                    {
                        QuestBoardOverallId = questBoardOverall.Id,
                        StudentId = studentId,
                        CurrentValue = energy ?? 0,
                        Token = questBoardOverall.Token,
                        Status = EnumQuestBoardOverallStudentStatus.NotReceived
                    });
                }
                else
                {
                    questBoardOverallStudent.CurrentValue += energy ?? 0;
                }
                await _questBoardOverallStudentRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
            }
        }
    }
}