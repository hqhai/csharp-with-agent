// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Queries.LocationQuery
{
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
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
        private readonly ICrmLocationRepository _locationCrmRepository;

        public GetLocationsQueryHandler(ILocationRepository locationRepository, ICrmLocationRepository locationCrmRepository)
        {
            _locationRepository = locationRepository;
            _locationCrmRepository = locationCrmRepository;
        }

        public async Task<MethodResult<PagingItemsModel<LocationModel>>> Handle(GetLocationsQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<PagingItemsModel<LocationModel>>();

            if (request.LocationType != EnumLocationType.Province && request.LocationType != EnumLocationType.District)
            {
                return methodResult;
            }

            var level = GetLevel(request.LocationType);

            var locations = _locationCrmRepository.Queryable.Where(p => p.Level == level);

            if (request.ParentId.HasValue)
            {
                var parent = _locationCrmRepository.Queryable.FirstOrDefault(p => p.GlobalId == request.ParentId);
                if (parent == null)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                    return methodResult;
                }

                locations = locations.Where(p => p.ParentId == parent.Id);
            }

            if (!string.IsNullOrEmpty(request.Keyword))
            {
                locations = locations.Where(m => m.Id.ToString() == request.Keyword || (m.Name ?? string.Empty).ToLower().Trim().Contains(request.Keyword.ToLower().Trim()));
            }

            var model = locations.Select(p => new LocationModel
            {
                Id = p.GlobalId,
                Name = p.Name,
            });

            int totalItem = model.Count();
            var lists = await model
                    .ApplySortAndPaging(request)
                    .OrderBy(x => x.Name)
                    .AsNoTracking()
                    .ToListAsync(cancellationToken: cancellationToken)
                    .ConfigureAwait(false);

            methodResult.Result = new PagingItemsModel<LocationModel>(model.ToList(), request, totalItem);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }

        private static int GetLevel(EnumLocationType locationType)
        {
            if (locationType == EnumLocationType.Province)
            {
                return 2;
            }
            else
            {
                return 3;
            }
        }
    }
}
