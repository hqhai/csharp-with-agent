// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Commands.QuestBoardStudentCmd
{
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Enums.ErrorCodes;
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
        private readonly IUserService _userService;
        private readonly IQuestBoardRepository _questBoardRepository;
        private readonly IQuestBoardStudentRepository _questBoardStudentRepository;
        private readonly AuthContext _authContext;

        public QuestBoardStudentCommandHandler(IUserService userService
            , IQuestBoardRepository questBoardRepository
            , IQuestBoardStudentRepository questBoardStudentRepository
            , AuthContext authContext)
        {
            _userService = userService;
            _questBoardRepository = questBoardRepository;
            _questBoardStudentRepository = questBoardStudentRepository;
            _authContext = authContext;
        }

        public async Task<MethodResult<bool>> Handle(QuestBoardStudentCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<bool>();
            var studentResult = await _userService.GetStudentByUserIdAsync(_authContext.CurrentUserId);
            if (!studentResult.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumServicesErrorCode.CallUserServiceError), nameof(studentResult));
                return methodResult;
            }
            var student = studentResult?.Content?.Result;
            if (student == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(student));
                return methodResult;
            }

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
