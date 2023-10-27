// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Querys.ForbiddenWordQuery
{
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Core.Extensions;
    using Fsel.System.Domain.IRepositories;
    using Fsel.System.Domain.Models.EntityModels;
    using Fsel.System.Domain.Models.QueryModels;
    using global::System.Globalization;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class SearchForbiddenWordQuery : SearchForbiddenWordQueryModel, IRequest<MethodResult<PagingItemsModel<ForbiddenWordModel>>>
    {
    }

    public class SearchForbiddenWordQueryHandler : IRequestHandler<SearchForbiddenWordQuery, MethodResult<PagingItemsModel<ForbiddenWordModel>>>
    {
        private readonly IForbiddenWordRepository _forbiddenWordRepository;

        public SearchForbiddenWordQueryHandler(IForbiddenWordRepository forbiddenWordRepository)
        {
            _forbiddenWordRepository = forbiddenWordRepository;
        }

        public async Task<MethodResult<PagingItemsModel<ForbiddenWordModel>>> Handle(SearchForbiddenWordQuery request, CancellationToken cancellationToken)
        {
            MethodResult<PagingItemsModel<ForbiddenWordModel>> methodResult = new MethodResult<PagingItemsModel<ForbiddenWordModel>>();
            ArgumentNullException.ThrowIfNull(request);
            if (request.PageSize > 100)
            {
                methodResult.StatusCode = StatusCodes.Status400BadRequest;
                return methodResult;
            }
            var forbiddenWordQuery = _forbiddenWordRepository.Queryable
                                .Select(x => new ForbiddenWordModel
                                {
                                    Id = x.Id,
                                    CreatedDate = x.CreatedDate,
                                    Description = x.Description,
                                    Word = x.Word,
                                });
            if (!string.IsNullOrEmpty(request.Keyword))
            {
                forbiddenWordQuery = forbiddenWordQuery.Where(m => m.Id.ToString() == request.Keyword || (m.Word ?? string.Empty).ToLower().Trim().Contains(request.Keyword.ToLower().Trim()));
            }
            int totalItem = await forbiddenWordQuery.CountAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            var lists = await forbiddenWordQuery
                    .ApplySortAndPaging(request)
                    .AsNoTracking()
                    .ToListAsync(cancellationToken: cancellationToken)
                    .ConfigureAwait(false);
            methodResult.Result = new PagingItemsModel<ForbiddenWordModel>(lists, request, totalItem);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
