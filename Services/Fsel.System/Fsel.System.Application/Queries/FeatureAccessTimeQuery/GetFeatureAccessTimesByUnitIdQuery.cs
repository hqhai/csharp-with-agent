// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Queries.FeatureAccessTimeQuery
{
    using Fsel.Common.ActionResults;
    using Fsel.System.Domain.IRepositories;
    using Fsel.System.Domain.Models.EntityModels;
    using global::System.Linq;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetFeatureAccessTimesByUnitIdQuery : IRequest<MethodResult<FeatureAccessTimeModel>>
    {
        public Guid CourseId { get; set; }
        public IList<Guid>? UnitIds { get; set; }
        public Guid UserId { get; set; }
    }

    public class GetFeatureAccessTimesByUnitIdQueryHandler : IRequestHandler<GetFeatureAccessTimesByUnitIdQuery, MethodResult<FeatureAccessTimeModel>>
    {
        private readonly IFeatureAccessTimeRepository _featureAccessTimeRepository;

        public GetFeatureAccessTimesByUnitIdQueryHandler(IFeatureAccessTimeRepository featureAccessTimeRepository)
        {
            _featureAccessTimeRepository = featureAccessTimeRepository;
        }

        public async Task<MethodResult<FeatureAccessTimeModel>> Handle(GetFeatureAccessTimesByUnitIdQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<FeatureAccessTimeModel> methodResult = new MethodResult<FeatureAccessTimeModel>();
            if (request.UnitIds == null || !request.UnitIds.Any())
            {
                methodResult.Result = null;
                methodResult.StatusCode = StatusCodes.Status200OK;
                return methodResult;
            }
            var featureAccessTimes = await _featureAccessTimeRepository.Queryable.Where(x => x.CreatedUserId == request.UserId && x.CourseId == request.CourseId && request.UnitIds.Contains(x.UnitId ?? default))
                .GroupBy(x => x.CourseId)
                .Select(x => new FeatureAccessTimeModel
                {
                    CourseId = x.Key,
                    UnitId = x.FirstOrDefault()!.UnitId,
                    AccessTime = x.Sum(x => x.AccessTime),
                    Visit = x.Sum(x => x.Visit),
                    LastVisited = x.OrderByDescending(x => x.LastVisited).FirstOrDefault()!.LastVisited ?? DateTime.Now,
                }).FirstOrDefaultAsync(cancellationToken);
            methodResult.Result = featureAccessTimes;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
