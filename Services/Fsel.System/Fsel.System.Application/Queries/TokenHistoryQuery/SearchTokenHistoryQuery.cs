// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Queries.TokenHistoryQuery
{
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base.BaseModels;
    using Fsel.System.Application.Queries.SchoolQuery;
    using Fsel.System.Domain.IRepositories;
    using Fsel.System.Domain.Models.EntityModels;
    using Fsel.System.Domain.Models.QueryModels;
    using MediatR;

    public class SearchTokenHistoryQuery : SearchTokenHistoryModel, IRequest<MethodResult<PagingItemsModel<TokenHistoryModel>>>
    {
    }
    public class SearchTokenHistoryQueryHandler : IRequestHandler<SearchTokenHistoryQuery, MethodResult<PagingItemsModel<TokenHistoryModel>>>
    {
        private readonly ITokenHistoryRepository _tokenHistoryRepository;

        public SearchTokenHistoryQueryHandler(ITokenHistoryRepository tokenHistoryRepository)
        {
            _tokenHistoryRepository = tokenHistoryRepository;
        }

        public Task<MethodResult<PagingItemsModel<TokenHistoryModel>>> Handle(SearchTokenHistoryQuery request, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
