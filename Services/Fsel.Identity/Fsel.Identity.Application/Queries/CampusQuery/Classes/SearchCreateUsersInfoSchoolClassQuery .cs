// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Queries.CampusQuery.Classes
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Identity.Domain.Entities.Campus;
    using Fsel.Identity.Domain.IRepositories;
    using Fsel.Shared.Models.ShareModels;
    using MediatR;

    public class SearchCreateUsersInfoSchoolClassQuery : BaseQueryModel, IRequest<MethodResult<PagingItemsModel<EntityModel>>>
    {
    }

    public class SearchCreateUsersInfoSchoolClassQueryHandler : IRequestHandler<SearchCreateUsersInfoSchoolClassQuery, MethodResult<PagingItemsModel<EntityModel>>>
    {
        private IGenericRepository<SchoolClass> _genericRepository;

        public SearchCreateUsersInfoSchoolClassQueryHandler(IGenericRepository<SchoolClass> genericRepository)
        {
            _genericRepository = genericRepository;
        }

        public async Task<MethodResult<PagingItemsModel<EntityModel>>> Handle(SearchCreateUsersInfoSchoolClassQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<PagingItemsModel<EntityModel>>();

            var results = await _genericRepository.SearchCreatedUsersInfoAsync(request);

            methodResult.Result = results;
            return methodResult;
        }
    }
}
