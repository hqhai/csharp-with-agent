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

    public class GetFeatureAccessTimesByUnitIdQuery : IRequest<MethodResult<IList<FeatureAccessTimeModel>>>
    {
        public Guid CourseId { get; set; }
        public IList<Guid> UnitIds { get; set; } = new List<Guid>();
        public Guid UserId { get; set; }
    }

    public class GetFeatureAccessTimesByUnitIdQueryHandler : IRequestHandler<GetFeatureAccessTimesByUnitIdQuery, MethodResult<IList<FeatureAccessTimeModel>>>
    {
        private readonly IFeatureAccessTimeRepository _featureAccessTimeRepository;

        public GetFeatureAccessTimesByUnitIdQueryHandler(IFeatureAccessTimeRepository featureAccessTimeRepository)
        {
            _featureAccessTimeRepository = featureAccessTimeRepository;
        }

        public async Task<MethodResult<IList<FeatureAccessTimeModel>>> Handle(GetFeatureAccessTimesByUnitIdQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<IList<FeatureAccessTimeModel>> methodResult = new MethodResult<IList<FeatureAccessTimeModel>>();
            var featureAccessTimes = await _featureAccessTimeRepository.Queryable.Where(x => x.CreatedUserId == request.UserId && x.CourseId == request.CourseId && request.UnitIds.Contains(x.UnitId ?? default))
                .GroupBy(x => x.CourseId)
                .Select(x => new FeatureAccessTimeModel
                {
                    CourseId = x.Key,
                    UnitId = x.FirstOrDefault()!.UnitId,
                    AccessTime = x.Sum(x => x.AccessTime),
                    Visit = x.Sum(x => x.Visit),
                    LastVisited = x.OrderByDescending(x => x.LastVisited).FirstOrDefault()!.LastVisited ?? DateTime.Now,
                }).ToListAsync(cancellationToken);
            methodResult.Result = featureAccessTimes;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
