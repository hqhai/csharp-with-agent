// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Commands.QuestBoardStudentCmd
{
    using Fsel.Common.ActionResults;
    using Fsel.Shared.Enums;
    using Fsel.System.Application.Services.UserServices;
    using Fsel.System.Domain.Entities;
    using Fsel.System.Domain.IRepositories;
    using Fsel.System.Domain.Models.CommandModels.QuestBoards;
    using global::System.Linq;
    using global::System.Threading;
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
        private readonly IUserService _userService;

        public QuestBoardStudentCommandHandler(
            IQuestBoardRepository questBoardRepository,
            IQuestBoardStudentRepository questBoardStudentRepository,
            IQuestBoardConfigRepository questBoardConfigRepository,
            IUserService userService)
        {
            _questBoardRepository = questBoardRepository;
            _questBoardStudentRepository = questBoardStudentRepository;
            _questBoardConfigRepository = questBoardConfigRepository;
            _userService = userService;
        }

        public async Task<MethodResult<bool>> Handle(QuestBoardStudentCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<bool>();

            #region CustomDataBeforeSave
            var (questBoardStudentToAdd, questBoardStudentToUpdate) = await CustomizeQuestBoardStudent(request, cancellationToken);
            #endregion

            #region Validate
            if (request.Categories == null || request.Categories.Count == 0)
            {
                methodResult.Result = true;
                return methodResult;
            }
            #endregion

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


        /// <summary>
        /// Filter dữ liệu listQuestBoard dựa vào packageid của học sinh 
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task<List<QuestBoard>> FilterListQuestBoardByPackageId(QuestBoardStudentCommand request, CancellationToken cancellationToken)
        {
            var questBoardQuery = _questBoardRepository.Queryable
                .Where(x => request.Categories!.Contains(x.Category));

            var listQuestBoard = await questBoardQuery.ToListAsync(cancellationToken);

            IList<Guid> studentIds = new List<Guid> { request.StudentId };
            var studentInfo = await _userService.GetStudentsByStudentIdsAsync(studentIds);
            var studentInfoPackageId = studentInfo.Content?.Result?.FirstOrDefault()?.PackageId;

            if (studentInfo != null && studentInfoPackageId.HasValue)
            {
                listQuestBoard = listQuestBoard.Where(x => x.PackageIds!.Contains((Guid)studentInfoPackageId)).ToList();
            }

            return listQuestBoard;
        }


        /// <summary>
        /// Custom dữ liệu trước khi Thêm, Cập nhật vào bảng QuestBoardStudent
        /// </summary>
        /// <param name="request"></param>
        /// <param name="listQuestBoard"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task<(List<QuestBoardStudent> questBoardStudentToUpdate, List<QuestBoardStudent> questBoardStudentToAdd)> CustomizeQuestBoardStudent(QuestBoardStudentCommand request, CancellationToken cancellationToken)
        {
            var questBoardStudentToUpdate = new List<QuestBoardStudent>();
            var questBoardStudentToAdd = new List<QuestBoardStudent>();
            var listQuestBoard = await FilterListQuestBoardByPackageId(request, cancellationToken);


            var questBoardStudents = await _questBoardStudentRepository.Queryable
                .Where(x => x.StudentId == request.StudentId && x.Status != EnumQuestBoardStudentStatus.Achieved)
                .ToListAsync(cancellationToken);
            var questBoardConfigQuery = _questBoardConfigRepository.Queryable
                .Where(x => request.Categories!.Contains(x.Category));

            foreach (var item in request.Categories!)
            {
                QuestBoardStudent? questBoardStudentUpdate = questBoardStudents.FirstOrDefault(x => x.QuestBoard?.Category == item);
                var questType = questBoardConfigQuery.FirstOrDefault(x => x.Category == item)?.Type;

                switch (questType)
                {
                    case EnumQuestBoardType.MainQuests:
                        questBoardStudentUpdate = CustomizeDataMainQuestBoard(questBoardStudents, item, request);
                        break;
                    case EnumQuestBoardType.DailyQuests:
                        questBoardStudentUpdate = CustomizeDataDailyQuestBoard(questBoardStudents, item, request);
                        break;
                }


                //xử lý thêm, sửa
                if (questBoardStudentUpdate != null) // case update
                {
                    if (questBoardStudentUpdate.AchievedPoints >= request.AchievedPoints)
                    {
                        continue;
                    }

                    questBoardStudentUpdate.AchievedPoints = request.AchievedPoints;
                    questBoardStudentToUpdate.Add(questBoardStudentUpdate);
                }
                else // case add
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

            return (questBoardStudentToUpdate, questBoardStudentToAdd);
        }
    }
}
