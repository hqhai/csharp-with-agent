// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Queries.LocationQuery
{
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Core.Extensions;
    using Fsel.System.Domain.Entities;
    using Fsel.System.Domain.IRepositories;
    using Fsel.System.Domain.Models.QueryModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetLocationFromCrmQuery : GetLocationFromCrmQueryModel, IRequest<MethodResult<PagingItemsModel<CrmLocation>?>>
    {
    }

    public class GetLocationFromCrmQueryHandler : IRequestHandler<GetLocationFromCrmQuery, MethodResult<PagingItemsModel<CrmLocation>?>>
    {
        private readonly ICrmLocationRepository _locationCrmRepository;

        public GetLocationFromCrmQueryHandler(ICrmLocationRepository locationCrmRepository)
        {
            _locationCrmRepository = locationCrmRepository;
        }

        public async Task<MethodResult<PagingItemsModel<CrmLocation>?>> Handle(GetLocationFromCrmQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<PagingItemsModel<CrmLocation>?>();

            var locations = _locationCrmRepository.Queryable.Where(p => p.Level == request.Level);

            if (request.ParentID.HasValue)
            {
                locations = locations.Where(p => p.ParentID == request.ParentID);
            }

            if (request.TypeLevel.HasValue)
            {
                if (request.TypeLevel == EnumCrmLocationTypeLevel.PrimarySchools)
                {
                    locations = locations.Where(p => p.TypeLevel == EnumCrmLocationTypeLevel.PrimarySchools || p.TypeLevel == EnumCrmLocationTypeLevel.PrimarySchoolsType0 || !p.TypeLevel.HasValue);
                }
                else
                {
                    locations = locations.Where(p => p.TypeLevel == request.TypeLevel);
                }
            }

            int totalItem = await locations.CountAsync(cancellationToken: cancellationToken).ConfigureAwait(false);

            var lists = await locations
                   .ApplySortAndPaging(request)
                   .AsNoTracking()
                   .ToListAsync(cancellationToken: cancellationToken)
                   .ConfigureAwait(false);

            methodResult.Result = new PagingItemsModel<CrmLocation>(lists, request, totalItem);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
