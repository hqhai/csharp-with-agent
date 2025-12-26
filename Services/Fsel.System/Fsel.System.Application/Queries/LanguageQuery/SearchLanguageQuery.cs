// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Queries.LanguageQuery
{
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base.BaseModels;
    using Fsel.System.Domain.IRepositories;
    using Fsel.System.Domain.Models.EntityModels;
    using global::System;
    using global::System.Linq;
    using global::System.Threading.Tasks;
    using MediatR;

    public class SearchLanguageQuery : BaseQueryModel, IRequest<MethodResult<PagingItemsModel<LanguageModel>>>
    {
    }

    public class SearchLanguageQueryHandler : IRequestHandler<SearchLanguageQuery, MethodResult<PagingItemsModel<LanguageModel>>>
    {
        private readonly ILanguageRepository _languageRepository;

        public SearchLanguageQueryHandler(ILanguageRepository languageRepository)
        {
            _languageRepository = languageRepository;
        }

        public async Task<MethodResult<PagingItemsModel<LanguageModel>>> Handle(SearchLanguageQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            var querys = _languageRepository.ReadQueryable;

            if (!string.IsNullOrEmpty(request.Keyword))
            {
                querys = querys.Where(x => !string.IsNullOrEmpty(x.Name) && x.Name.Trim() == request.Keyword.Trim());
            }

            return await _languageRepository.GetListByPageResultAsync<LanguageModel>(querys, request, cancellationToken);
        }
    }
}
