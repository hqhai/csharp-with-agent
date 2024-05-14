// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Queries.LocationQuery
{
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Core.Extensions;
    using Fsel.Shared.Enums;
    using Fsel.System.Domain.IRepositories;
    using Fsel.System.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetLocationsQuery : BaseQueryModel, IRequest<MethodResult<PagingItemsModel<LocationModel>>>
    {
        public EnumLocationType LocationType { get; set; }
        public Guid? ParentId { get; set; }
    }

    public class GetLocationsQueryHandler : IRequestHandler<GetLocationsQuery, MethodResult<PagingItemsModel<LocationModel>>>
    {
        private readonly ILocationRepository _locationRepository;

        public GetLocationsQueryHandler(ILocationRepository locationRepository)
        {
            _locationRepository = locationRepository;
        }

        public async Task<MethodResult<PagingItemsModel<LocationModel>>> Handle(GetLocationsQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<PagingItemsModel<LocationModel>>();

            var locations = _locationRepository.Queryable.Where(p => p.Type == request.LocationType).Select(p => new LocationModel()
            {
                Id = p.Id,
                Code = p.UrBoxId,
                Name = p.Name,
                ParentId = p.ParentId,
                CreatedDate = p.CreatedDate,
            });

            if (!string.IsNullOrEmpty(request.Keyword))
            {
                locations = locations.Where(m => m.Id.ToString() == request.Keyword || (m.Name ?? string.Empty).ToLower().Trim().Contains(request.Keyword.ToLower().Trim()));
            }

            if (request.ParentId.HasValue)
            {
                locations = locations.Where(p => p.ParentId == request.ParentId);
            }

            int totalItem = await locations.CountAsync(cancellationToken: cancellationToken).ConfigureAwait(false);

            var lists = await locations
                    .ApplySortAndPaging(request)
                    .OrderBy(x => x.Name)
                    .AsNoTracking()
                    .ToListAsync(cancellationToken: cancellationToken)
                    .ConfigureAwait(false);

            methodResult.Result = new PagingItemsModel<LocationModel>(lists, request, totalItem);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
