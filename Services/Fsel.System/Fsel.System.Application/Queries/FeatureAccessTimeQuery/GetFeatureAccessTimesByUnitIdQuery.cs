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

    public class GetFeatureAccessTimesByUnitIdQuery : IRequest<MethodResult<FeatureAccessTimeCourseModel>>
    {
        public Guid CourseId { get; set; }
        public IList<Guid>? UnitIds { get; set; }
        public Guid UserId { get; set; }
    }

    public class GetFeatureAccessTimesByUnitIdQueryHandler : IRequestHandler<GetFeatureAccessTimesByUnitIdQuery, MethodResult<FeatureAccessTimeCourseModel>>
    {
        private readonly IFeatureAccessTimeRepository _featureAccessTimeRepository;

        public GetFeatureAccessTimesByUnitIdQueryHandler(IFeatureAccessTimeRepository featureAccessTimeRepository)
        {
            _featureAccessTimeRepository = featureAccessTimeRepository;
        }

        public async Task<MethodResult<FeatureAccessTimeCourseModel>> Handle(GetFeatureAccessTimesByUnitIdQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<FeatureAccessTimeCourseModel> methodResult = new MethodResult<FeatureAccessTimeCourseModel>();
            if (request.UnitIds == null || !request.UnitIds.Any())
            {
                methodResult.Result = null;
                methodResult.StatusCode = StatusCodes.Status200OK;
                return methodResult;
            }
            var featureAccessTimes = await _featureAccessTimeRepository.Queryable.Where(x => x.CreatedUserId == request.UserId && x.CourseId == request.CourseId && request.UnitIds.Contains(x.UnitId ?? default))
                .GroupBy(x => x.CourseId)
                .Select(x => new FeatureAccessTimeCourseModel
                {
                    CourseId = x.Key,
                    AccessTime = x.Sum(x => x.AccessTime),
                    TotalVisit = x.Sum(x => x.Visit),
                    LastVisited = x.OrderByDescending(x => x.LastVisited).FirstOrDefault() != null ? x.OrderByDescending(x => x.LastVisited).FirstOrDefault()!.LastVisited : default,
                }).FirstOrDefaultAsync(cancellationToken);
            methodResult.Result = featureAccessTimes;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
