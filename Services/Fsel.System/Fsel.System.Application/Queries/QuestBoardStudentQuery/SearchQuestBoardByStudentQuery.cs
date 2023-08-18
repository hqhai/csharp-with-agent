// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Queries.QuestBoardStudentQuery
{
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Core.Extensions;
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
        private readonly IQuestBoardStudentRepository _questBoardStudentRepository;
        private readonly AuthContext _authContext;
        private readonly IUserService _userService;

        public SearchQuestBoardByStudentQueryHandler(IQuestBoardRepository questBoardRepository
            , IQuestBoardStudentRepository questBoardStudentRepository
            , AuthContext authContext
            , IUserService userService)
        {
            _questBoardRepository = questBoardRepository;
            _questBoardStudentRepository = questBoardStudentRepository;
            _authContext = authContext;
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
            var questBoardQuery = from baseQ in _questBoardRepository.Queryable
                                  join qts in _questBoardStudentRepository.Queryable on baseQ.Id equals qts.QuestBoardId
                                  where baseQ.Type == request.Type && (baseQ.PackageIds != null && baseQ.PackageIds.Any(x => x == student.PackageId)) && (qts == null || qts.StudentId == student.Id)
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
