// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Application.Queries.FlagQuery
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Interaction.Domain.IRepositories;
    using Fsel.Interaction.Domain.Models.EntityModels;
    using Fsel.Interaction.Domain.Models.QueryModels.Flags;
    using Fsel.Shared.Enums;
    using MediatR;

    public class SearchFlagQuery : SearchFlagQueryModel, IRequest<MethodResult<PagingItemsModel<FlagModel>>>
    {
    }

    public class SearchFlagQueryHandler : IRequestHandler<SearchFlagQuery, MethodResult<PagingItemsModel<FlagModel>>>
    {
        private readonly IFlagRepository _flagRepository;

        public SearchFlagQueryHandler(IFlagRepository flagRepository)
        {
            _flagRepository = flagRepository;
        }

        public async Task<MethodResult<PagingItemsModel<FlagModel>>> Handle(SearchFlagQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var flagQuery = _flagRepository.Queryable.Where(x => x.Status == EnumFlagStatus.New);
            return await _flagRepository.GetListByPageResultAsync<FlagModel>(flagQuery, request, cancellationToken).ConfigureAwait(false);
        }
    }
}
