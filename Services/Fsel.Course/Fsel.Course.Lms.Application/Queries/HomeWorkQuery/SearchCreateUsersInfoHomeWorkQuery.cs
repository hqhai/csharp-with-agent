// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.HomeWorkQuery
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Shared.Models.ShareModels;
    using MediatR;

    public class SearchCreateUsersInfoHomeWorkQuery : BaseQueryModel, IRequest<MethodResult<PagingItemsModel<EntityModel>>>
    {
    }

    public class SearchCreateUsersInfoHomeWorkQueryHandler : IRequestHandler<SearchCreateUsersInfoHomeWorkQuery, MethodResult<PagingItemsModel<EntityModel>>>
    {
        private IGenericRepository<HomeWork> _genericRepository;

        public SearchCreateUsersInfoHomeWorkQueryHandler(IGenericRepository<HomeWork> genericRepository)
        {
            _genericRepository = genericRepository;
        }

        public async Task<MethodResult<PagingItemsModel<EntityModel>>> Handle(SearchCreateUsersInfoHomeWorkQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<PagingItemsModel<EntityModel>>();

            var results = await _genericRepository.SearchCreatedUsersInfoAsync(request);

            methodResult.Result = results;
            return methodResult;
        }
    }
}
