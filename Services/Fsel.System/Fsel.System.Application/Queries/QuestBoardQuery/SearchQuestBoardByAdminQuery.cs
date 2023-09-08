// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Queries.QuestBoardQuery
{
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Core.Extensions;
    using Fsel.System.Domain.IRepositories;
    using Fsel.System.Domain.Models.EntityModels;
    using Fsel.System.Domain.Models.QueryModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class SearchQuestBoardByAdminQuery : SearchQuestBoardByAdminQueryModel, IRequest<MethodResult<PagingItemsModel<QuestBoardSearchModel>>>
    {
    }

    public class SearchQuestBoardByAdminQueryHandler : IRequestHandler<SearchQuestBoardByAdminQuery, MethodResult<PagingItemsModel<QuestBoardSearchModel>>>
    {
        private readonly IQuestBoardRepository _questBoardRepository;

        public SearchQuestBoardByAdminQueryHandler(IQuestBoardRepository questBoardRepository)
        {
            _questBoardRepository = questBoardRepository;
        }

        public async Task<MethodResult<PagingItemsModel<QuestBoardSearchModel>>> Handle(SearchQuestBoardByAdminQuery request, CancellationToken cancellationToken)
        {
            MethodResult<PagingItemsModel<QuestBoardSearchModel>> methodResult = new MethodResult<PagingItemsModel<QuestBoardSearchModel>>();
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
            var questBoardQuery = _questBoardRepository.Queryable.Where(x => x.Type == request.Type)
                                .Select(x => new QuestBoardSearchModel
                                {
                                    Id = x.Id,
                                    CreatedDate = x.CreatedDate,
                                    CreatedFullName = x.CreatedFullName,
                                    CreatedUserId = x.CreatedUserId,
                                    EndDate = x.EndDate ?? null,
                                    Name = x.Name,
                                    IsLifeTime = x.IsLifeTime,
                                    StartDate = x.StartDate,
                                    UpdatedDate = x.UpdatedDate,
                                    UpdatedFullName = x.UpdatedFullName,
                                    UpdatedUserId = x.UpdatedUserId,
                                    MissionType = x.DependentId != null,
                                    IsActive = x.IsActive
                                });
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
            methodResult.Result = new PagingItemsModel<QuestBoardSearchModel>(lists, request, totalItem);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
