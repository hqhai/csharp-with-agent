// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.HomeWorkConfigQuery
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Shared.Models.ShareModels;
    using MediatR;

    public class SearchCreateUsersInfoHomeWorkConfigQuery : BaseQueryModel, IRequest<MethodResult<PagingItemsModel<EntityModel>>>
    {
    }

    public class SearchCreateUsersInfoHomeWorkConfigQueryHandler : IRequestHandler<SearchCreateUsersInfoHomeWorkConfigQuery, MethodResult<PagingItemsModel<EntityModel>>>
    {
        private IGenericRepository<HomeWorkConfig> _genericRepository;

        public SearchCreateUsersInfoHomeWorkConfigQueryHandler(IGenericRepository<HomeWorkConfig> genericRepository)
        {
            _genericRepository = genericRepository;
        }

        public async Task<MethodResult<PagingItemsModel<EntityModel>>> Handle(SearchCreateUsersInfoHomeWorkConfigQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<PagingItemsModel<EntityModel>>();

            var results = await _genericRepository.SearchCreatedUsersInfoAsync(request);

            methodResult.Result = results;
            return methodResult;
        }
    }
}
