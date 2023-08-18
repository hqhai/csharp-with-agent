// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Queries.QuestBoardStudentQuery
{
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Enums.ErrorCodes;
    using Fsel.System.Application.Services.CourseServices;
    using Fsel.System.Application.Services.CourseServices.Models;
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
        private readonly IQuestBoardStudentRepository _questBoardStudentRepository;
        private readonly AuthContext _authContext;
        private readonly ICourseService _courseService;
        private readonly IUserService _userService;

        public SearchQuestBoardByStudentQueryHandler(IQuestBoardRepository questBoardRepository
            , IQuestBoardStudentRepository questBoardStudentRepository
            , AuthContext authContext
            , ICourseService courseService
            , IUserService userService)
        {
            _questBoardRepository = questBoardRepository;
            _questBoardStudentRepository = questBoardStudentRepository;
            _authContext = authContext;
            _courseService = courseService;
            _userService = userService;
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
                                     .Where(x => x.Type == request.Type && x.DependentId == null)
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
                                         PackageIds = baseQ.PackageIds,
                                         RepeatType = baseQ.RepeatType ?? null,
                                         Category = baseQ.Category,
                                         StartDate = baseQ.StartDate,
                                         EndDate = baseQ.EndDate ?? null,
                                         NumberOfStars = baseQ.NumberOfStars,
                                         Status = baseQ.QuestBoardStudents.FirstOrDefault(x => x.StudentId == student.Id && x.QuestBoardId == baseQ.Id) == null ? EnumQuestBoardStatus.New : baseQ.QuestBoardStudents.FirstOrDefault(x => x.StudentId == student.Id && x.QuestBoardId == baseQ.Id)!.Status,
                                     }).ToListAsync(cancellationToken);
            questBoards = questBoards.Where(baseQ => baseQ.PackageIds != null && baseQ.PackageIds.Count > 0 && baseQ.PackageIds.Any(x => x == student.PackageId)).ToList();
            var questBoardDepentDoneIds = questBoards.Where(x => x.Status == EnumQuestBoardStatus.Done).Select(x => x.Id).ToList();
            var questBoardDepentQuery = from baseQ in _questBoardRepository.Queryable
                                        join qts in _questBoardStudentRepository.Queryable on baseQ.Id equals qts.QuestBoardId
                                        where questBoardDepentDoneIds.Any(x => x == baseQ.Id)
                                        select new QuestBoardByStudentModel
                                        {
                                            Id = baseQ.Id,
                                            CreatedDate = baseQ.CreatedDate,
                                            CreatedFullName = baseQ.CreatedFullName,
                                            CreatedUserId = baseQ.CreatedUserId,
                                            UpdatedDate = baseQ.UpdatedDate,
                                            UpdatedFullName = baseQ.UpdatedFullName,
                                            UpdatedUserId = baseQ.UpdatedUserId,
                                            Name = baseQ.Name,
                                            RepeatType = baseQ.RepeatType ?? null,
                                            Category = baseQ.Category,
                                            StartDate = baseQ.StartDate,
                                            EndDate = baseQ.EndDate ?? null,
                                            NumberOfStars = baseQ.NumberOfStars,
                                            Status = qts == null ? EnumQuestBoardStatus.New : qts.Status,
                                        };

            var query = questBoards.Union(await questBoardDepentQuery.ToListAsync(cancellationToken));
            int totalItem = query.Count();
            var lists = query.OrderBy(x => x.CreatedDate).Skip((request!.Page - 1) * request!.PageSize).Take(request!.PageSize).ToList();
            switch (request.Type)
            {
                case EnumQuestBoardType.MainQuests:
                    var listQuestBoards = await GetQuestBoardsTypeMainQuests(lists, _authContext.CurrentUserId, student.Id);
                    lists = listQuestBoards.ToList();
                    break;

                case EnumQuestBoardType.EventQuests:
                    break;

                case EnumQuestBoardType.SideQuests:
                    break;

                case EnumQuestBoardType.PremiumQuests:
                    break;

                case EnumQuestBoardType.DailyQuests:
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

        private async Task<IList<QuestBoardByStudentModel>> GetQuestBoardsTypeMainQuests(IList<QuestBoardByStudentModel> lists, Guid currentUserId, Guid studentId)
        {
            foreach (var item in lists)
            {
                switch (item.Category)
                {
                    case EnumQuestBoardCategory.FinishOnelesson:
                        var finishOnelesson = await _courseService.GetPercentVideoResult(new GetFinishOneLessonQueryModel { EndDate = item.EndDate, StartDate = item.StartDate, RepeatType = item.RepeatType, StudentId = studentId });
                        item.Percent = finishOnelesson.Content?.Result ?? default;
                        break;

                    case EnumQuestBoardCategory.FinishOneUnit:
                        var finishOneUnit = await _courseService.GetPercentUnitResult(new GetFinishOneQueryModel { CurrentUserId = currentUserId, StudentId = studentId });
                        item.Percent = finishOneUnit.Content?.Result ?? default;
                        break;

                    case EnumQuestBoardCategory.FinishOneLevelPass:
                        var finishOneLevelPass = await _courseService.GetPercentCourseResult(new GetFinishOneQueryModel { CurrentUserId = currentUserId, StudentId = studentId });
                        item.Percent = finishOneLevelPass.Content?.Result ?? default;
                        break;

                    default:
                        break;
                }
            }

            return lists;
        }
    }
}
