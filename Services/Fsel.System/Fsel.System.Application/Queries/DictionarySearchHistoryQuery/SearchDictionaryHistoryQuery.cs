// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Queries.DictionarySearchHistoryQuery
{
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Core.Base;
    using Fsel.System.Infrastructure;
    using Fsel.System.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;
    using Fsel.Core.Extensions;

    public class SearchDictionaryHistoryQuery : BaseQueryModel, IRequest<MethodResult<PagingItemsModel<DictionarySearchHistoryModel>>>
    {
    }

    public class SearchDictionaryHistoryQueryHandler : IRequestHandler<SearchDictionaryHistoryQuery, MethodResult<PagingItemsModel<DictionarySearchHistoryModel>>>
    {
        private readonly PostgreDbContext _context;
        private readonly AuthContext _authContext;

        public SearchDictionaryHistoryQueryHandler(PostgreDbContext context, AuthContext authContext)
        {
            _context = context;
            _authContext = authContext;
        }

        public async Task<MethodResult<PagingItemsModel<DictionarySearchHistoryModel>>> Handle(
            SearchDictionaryHistoryQuery request,
            CancellationToken cancellationToken)
        {
            var methodResult = new MethodResult<PagingItemsModel<DictionarySearchHistoryModel>>();

            ArgumentNullException.ThrowIfNull(request);

            if (request.PageSize > 100)
            {
                methodResult.StatusCode = StatusCodes.Status400BadRequest;
                methodResult.AddError("INVALID_PAGE_SIZE", "Page size cannot exceed 100");
                return methodResult;
            }

            var query = _context.DictionarySearchHistories
                .Where(h => !h.IsDeleted && h.UserId == _authContext.CurrentUserId)
                .OrderByDescending(h => h.CreatedDate)
                .Select(h => new DictionarySearchHistoryModel
                {
                    Id = h.Id,
                    SearchTerm = h.SearchTerm,
                    SearchContext = h.SearchContext,
                    DictionaryAIId = h.DictionaryAIId,
                    SourceLanguage = h.SourceLanguage,
                    TargetLanguage = h.TargetLanguage,
                    FoundResult = h.FoundResult,
                    ResultCount = h.ResultCount,
                    ResponseTimeMs = h.ResponseTimeMs,
                    SearchedAt = h.CreatedDate
                });

            if (!string.IsNullOrEmpty(request.Keyword))
            {
                query = query.Where(m =>
                    m.SearchTerm.ToLower().Contains(request.Keyword.ToLower()) ||
                    (m.SearchContext != null && m.SearchContext.ToLower().Contains(request.Keyword.ToLower())));
            }

            var totalItem = await query.CountAsync(cancellationToken: cancellationToken).ConfigureAwait(false);

            var lists = await query
                        .ApplySortAndPaging(request)
                        .AsNoTracking()
                        .ToListAsync(cancellationToken: cancellationToken)
                        .ConfigureAwait(false);

            methodResult.Result = new PagingItemsModel<DictionarySearchHistoryModel>(lists, request, totalItem);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
