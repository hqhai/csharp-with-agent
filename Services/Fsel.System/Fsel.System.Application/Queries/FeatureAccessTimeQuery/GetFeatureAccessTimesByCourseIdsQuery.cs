// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Queries.FeatureAccessTimeQuery
{
    using Fsel.Common.ActionResults;
    using Fsel.System.Domain.IRepositories;
    using Fsel.System.Domain.Models.EntityModels;
    using Fsel.System.Domain.Models.QueryModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetFeatureAccessTimesByCourseIdsQuery : GetFeatureAccessTimesByCourseIdsQueryModel, IRequest<MethodResult<IList<FeatureAccessTimeCourseModel>>>
    {
    }

    public class GetFeatureAccessTimesByCourseIdsQueryHandler : IRequestHandler<GetFeatureAccessTimesByCourseIdsQuery, MethodResult<IList<FeatureAccessTimeCourseModel>>>
    {
        private readonly IFeatureAccessTimeRepository _featureAccessTimeRepository;

        public GetFeatureAccessTimesByCourseIdsQueryHandler(IFeatureAccessTimeRepository featureAccessTimeRepository)
        {
            _featureAccessTimeRepository = featureAccessTimeRepository;
        }

        public async Task<MethodResult<IList<FeatureAccessTimeCourseModel>>> Handle(GetFeatureAccessTimesByCourseIdsQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<IList<FeatureAccessTimeCourseModel>> methodResult = new MethodResult<IList<FeatureAccessTimeCourseModel>>();
            if (request.CourseIds == null || !request.CourseIds.Any())
            {
                methodResult.Result = null;
                methodResult.StatusCode = StatusCodes.Status200OK;
                return methodResult;
            }
            var featureAccessTimes = await _featureAccessTimeRepository.Queryable.Where(x => x.CreatedUserId == request.UserId && request.CourseIds.Contains(x.CourseId))
                .GroupBy(x => x.CourseId)
                .Select(x => new FeatureAccessTimeCourseModel
                {
                    CourseId = x.Key,
                    AccessTime = x.Sum(x => x.AccessTime),
                    Visit = x.Sum(x => x.Visit),
                    LastVisited = x.OrderBy(x => x.CreatedDate).FirstOrDefault()!.LastVisited ?? DateTime.Now,
                }).ToListAsync(cancellationToken);
            methodResult.Result = featureAccessTimes;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
