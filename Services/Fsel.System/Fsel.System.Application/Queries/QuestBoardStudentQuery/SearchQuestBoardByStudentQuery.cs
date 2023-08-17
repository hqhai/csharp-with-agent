// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Queries.QuestBoardStudentQuery
{
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Core.Extensions;
    using Fsel.Shared.Enums;
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
        private readonly IQuestBoardTaskStudentRepository _questBoardTaskStudentRepository;
        private readonly IQuestBoardTaskRepository _questBoardTaskRepository;

        public SearchQuestBoardByStudentQueryHandler(IQuestBoardRepository questBoardRepository
            , AuthContext authContext
            , IUserService userService
            , IQuestBoardTaskStudentRepository questBoardTaskStudentRepository
            , IQuestBoardTaskRepository questBoardTaskRepository)
        {
            _questBoardRepository = questBoardRepository;
            _authContext = authContext;
            _userService = userService;
            _questBoardTaskStudentRepository = questBoardTaskStudentRepository;
            _questBoardTaskRepository = questBoardTaskRepository;
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
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(studentResult));
                return methodResult;
            }
            var student = studentResult?.Content?.Result;
            if (student == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(student));
                return methodResult;
            }
            var questBoardQuery = from baseQ in _questBoardRepository.Queryable
                                  join q in _questBoardTaskRepository.Queryable on baseQ.Id equals q.QuestBoardId
                                  join qts in _questBoardTaskStudentRepository.Queryable on q.Id equals qts.QuestBoardTaskId
                                  where baseQ.Type == request.Type && q.ImplementDate >= DateTime.Now && (baseQ.PackageIds != null && baseQ.PackageIds.Any(x => x == student.PackageId)) && (qts == null || qts.StudentId == student.Id)
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
                                      Category = baseQ.Category,
                                      NumberOfStars = baseQ.NumberOfStars,
                                      Status = qts == null ? EnumQuestBoardStatus.New : qts.Status,
                                  };

            if (!string.IsNullOrEmpty(request.Keyword))
            {
                questBoardQuery = questBoardQuery.Where(m => m.Id.ToString() == request.Keyword || (m.Name ?? string.Empty).Contains(request.Keyword));
            }
            int totalItem = await questBoardQuery.CountAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            var lists = await questBoardQuery
                    .ApplySortAndPaging(request)
                    .AsNoTracking()
                    .ToListAsync(cancellationToken: cancellationToken)
                    .ConfigureAwait(false);

            methodResult.Result = new PagingItemsModel<QuestBoardByStudentModel>(lists, request, totalItem);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
