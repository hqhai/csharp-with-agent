// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Queries.QuestBoardStudentQuery
{
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Core.Extensions;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Enums.ErrorCodes;
    using Fsel.System.Application.Services.UserServices;
    using Fsel.System.Domain.IRepositories;
    using Fsel.System.Domain.Models.EntityModels;
    using Fsel.System.Domain.Models.QueryModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class SearchQuestBoardByStudentQuery : SearchQuestBoardByStudentQueryModel, IRequest<MethodResult<PagingItemsModel<QuestBoardByStudentModel>>>
    {
    }

    public class SearchQuestBoardByStudentQueryHandler : IRequestHandler<SearchQuestBoardByStudentQuery, MethodResult<PagingItemsModel<QuestBoardByStudentModel>>>
    {
        private readonly IQuestBoardRepository _questBoardRepository;
        private readonly AuthContext _authContext;
        private readonly IUserService _userService;
        private readonly IQuestBoardConfigRepository _questBoardConfigRepository;

        public SearchQuestBoardByStudentQueryHandler(IQuestBoardRepository questBoardRepository
            , AuthContext authContext
            , IUserService userService
            , IQuestBoardConfigRepository questBoardConfigRepository)
        {
            _questBoardRepository = questBoardRepository;
            _authContext = authContext;
            _userService = userService;
            _questBoardConfigRepository = questBoardConfigRepository;
        }

        public async Task<MethodResult<PagingItemsModel<QuestBoardByStudentModel>>> Handle(SearchQuestBoardByStudentQuery request, CancellationToken cancellationToken)
        {
            MethodResult<PagingItemsModel<QuestBoardByStudentModel>> methodResult = new MethodResult<PagingItemsModel<QuestBoardByStudentModel>>();
            ArgumentNullException.ThrowIfNull(request);
            if (request.PageSize > 100)
            {
                methodResult.StatusCode = StatusCodes.Status400BadRequest;
                return methodResult;
            }

            if (request.Type == null)
            {
                methodResult.Result = null;
                methodResult.StatusCode = StatusCodes.Status200OK;
                return methodResult;
            }

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
            var questBoards = await _questBoardRepository.Queryable.Include(x => x.QuestBoardStudents)
                                     .Where(x => x.Type == request.Type)
                                     .Select(baseQ => new QuestBoardByStudentModel
                                     {
                                         Id = baseQ.Id,
                                         CreatedDate = baseQ.CreatedDate,
                                         CreatedFullName = baseQ.CreatedFullName,
                                         CreatedUserId = baseQ.CreatedUserId,
                                         UpdatedDate = baseQ.UpdatedDate,
                                         UpdatedFullName = baseQ.UpdatedFullName,
                                         UpdatedUserId = baseQ.UpdatedUserId,
                                         Name = baseQ.Name,
                                         QuestBoardType = baseQ.Type,
                                         AchievedPoints = baseQ.QuestBoardStudents.Any(x => x.QuestBoardId == baseQ.Id && x.StudentId == student.Id) ? baseQ.QuestBoardStudents.FirstOrDefault(x => x.QuestBoardId == baseQ.Id && x.StudentId == student.Id)!.AchievedPoints : 0,
                                         PackageIds = baseQ.PackageIds,
                                         RepeatType = baseQ.RepeatType ?? null,
                                         Category = baseQ.Category,
                                         StartDate = baseQ.StartDate,
                                         EndDate = baseQ.EndDate ?? null,
                                         NumberOfStars = baseQ.NumberOfStars,
                                         Status = baseQ.QuestBoardStudents.Any(x => x.StudentId == student.Id && x.QuestBoardId == baseQ.Id) ? baseQ.QuestBoardStudents.FirstOrDefault(x => x.StudentId == student.Id && x.QuestBoardId == baseQ.Id)!.Status : EnumQuestBoardStudentStatus.Process
                                     }).ToListAsync(cancellationToken);

            questBoards = questBoards.Where(baseQ => baseQ.PackageIds != null && baseQ.PackageIds.Count > 0 && baseQ.PackageIds.Any(x => x == student.PackageId)).ToList();

            var questBoardConfig = _questBoardConfigRepository.Queryable.Where(x => x.Type == request.Type).ToList();

            var joinedQuests = from quest in questBoards
                               join config in questBoardConfig on quest.Category equals config.Category
                               select new
                               {
                                   quest,
                                   MaxPoints = config.MaxPoints
                               };

            // Cập nhật giá trị MaxPoint trong questboard
            foreach (var result in joinedQuests)
            {
                result.quest.MaxPoints = result.MaxPoints;
            }

            int totalItem = questBoards.Count;
            var lists = questBoards.ApplySortAndPaging(request).ToList();



            switch (request.Type)
            {
                case EnumQuestBoardType.MainQuests:
                    lists = questBoards.Where(x => x.QuestBoardType == EnumQuestBoardType.MainQuests).ToList();
                    break;
                case EnumQuestBoardType.EventQuests:
                    break;

                case EnumQuestBoardType.SideQuests:
                    lists = questBoards.Where(x => x.QuestBoardType == EnumQuestBoardType.SideQuests).ToList();
                    break;
                case EnumQuestBoardType.PremiumQuests:
                    lists = questBoards.Where(x => x.QuestBoardType == EnumQuestBoardType.PremiumQuests).ToList();
                    break;

                case EnumQuestBoardType.DailyQuests:
                    Random random = new Random();
                    lists = questBoards.Where(x => x.QuestBoardType == EnumQuestBoardType.DailyQuests).OrderBy(x => random.Next()).Take(ValueSettings.QuestBoardPoint.Random_Daily_QuestBoard).ToList();
                    break;

                case EnumQuestBoardType.FunChallenges:
                    break;

                case EnumQuestBoardType.TreasureHunters:
                    break;
            }

            methodResult.Result = new PagingItemsModel<QuestBoardByStudentModel>(lists, request, totalItem);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
