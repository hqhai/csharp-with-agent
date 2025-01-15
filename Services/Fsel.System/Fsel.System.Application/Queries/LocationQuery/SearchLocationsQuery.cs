// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Queries.LocationQuery
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Core.Extensions;
    using Fsel.Shared.Enums;
    using Fsel.System.Domain.Entities;
    using Fsel.System.Domain.IRepositories;
    using Fsel.System.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class SearchLocationsQuery : BaseQueryModel, IRequest<MethodResult<PagingItemsModel<LocationModel>>>
    {
        public EnumLocationType LocationType { get; set; }
        public Guid? ParentId { get; set; }
    }

    public class SearchLocationsQueryHandler : IRequestHandler<SearchLocationsQuery, MethodResult<PagingItemsModel<LocationModel>>>
    {
        private readonly IMapper _mapper;
        private readonly ICrmLocationRepository _locationCrmRepository;

        public SearchLocationsQueryHandler(ICrmLocationRepository locationCrmRepository, IMapper mapper)
        {
            _locationCrmRepository = locationCrmRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<PagingItemsModel<LocationModel>>> Handle(SearchLocationsQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<PagingItemsModel<LocationModel>>();

            //if (request.LocationType != EnumLocationType.Province && request.LocationType != EnumLocationType.District)
            //{
            //    return methodResult;
            //}

            var locations = _locationCrmRepository.Queryable.Where(p => p.Level.HasValue && p.Level == (EnumCrmLocationLevel)request.LocationType);

            if (request.ParentId.HasValue)
            {
                var parent = _locationCrmRepository.Queryable.FirstOrDefault(p => p.GlobalId == request.ParentId);
                if (parent == null)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                    return methodResult;
                }
                if (parent.Level == EnumCrmLocationLevel.Country)
                {
                    locations = locations.Where(p => p.Parent != null && p.Parent.ParentId == parent.Id);
                }
                else
                {
                    locations = locations.Where(p => p.ParentId == parent.Id);
                }
            }

            if (!string.IsNullOrEmpty(request.Keyword))
            {
                locations = locations.Where(m => m.GlobalId.ToString() == request.Keyword || (m.Name ?? string.Empty).ToLower().Trim().Contains(request.Keyword.ToLower().Trim()));
            }

            int totalItem = await locations.CountAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            var lists = await locations
                    .ApplySortAndPaging(request)
                    .OrderBy(x => x.Name)
                    .AsNoTracking()
                    .ToListAsync(cancellationToken: cancellationToken)
                    .ConfigureAwait(false);

            methodResult.Result = new PagingItemsModel<LocationModel>(_mapper.Map<List<LocationModel>>(lists), request, totalItem);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
