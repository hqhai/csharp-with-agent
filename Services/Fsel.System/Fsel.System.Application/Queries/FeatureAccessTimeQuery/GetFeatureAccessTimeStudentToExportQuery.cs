// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Queries.FeatureAccessTimeQuery
{
    using Fsel.Common.ActionResults;
    using Fsel.Shared.Models.ShareModels.QueryModels;
    using Fsel.System.Domain.IRepositories;
    using Fsel.System.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetFeatureAccessTimeStudentToExportQuery : GetFeatureAccessTimeToExportQueryModel, IRequest<MethodResult<IList<FeatureAccessTimeModel>>>
    {
    }

    public class GetFeatureAccessTimeStudentToExportQueryHandler : IRequestHandler<GetFeatureAccessTimeStudentToExportQuery, MethodResult<IList<FeatureAccessTimeModel>>>
    {
        private readonly IFeatureAccessTimeRepository _featureAccessTimeRepository;

        public GetFeatureAccessTimeStudentToExportQueryHandler(IFeatureAccessTimeRepository featureAccessTimeRepository)
        {
            _featureAccessTimeRepository = featureAccessTimeRepository;
        }

        public async Task<MethodResult<IList<FeatureAccessTimeModel>>> Handle(GetFeatureAccessTimeStudentToExportQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<IList<FeatureAccessTimeModel>> methodResult = new MethodResult<IList<FeatureAccessTimeModel>>();
            if (request.FeatureAccessTimes == null || !request.FeatureAccessTimes.Any())
            {
                return methodResult;
            }

            var featureAccessTimeRequestEnumFeatures = request.FeatureAccessTimes.Where(x => x.EnumFeature.HasValue && !x.CourseId.HasValue)
               .Select(f => new
               {
                   f.UserId,
                   EnumFeature = f.EnumFeature.GetValueOrDefault(),
               }).ToList();

            var userIdEnums = featureAccessTimeRequestEnumFeatures.Select(x => x.UserId).Distinct().ToList();
            var featureEnums = featureAccessTimeRequestEnumFeatures.Select(x => x.EnumFeature).Distinct().ToList();

            var featureAccessTimeEnumFeatures = await (from baseQ in _featureAccessTimeRepository.Queryable.WhereBulkContains(userIdEnums, x => x.CreatedUserId).Where(x => featureEnums.Contains(x.EnumFeature))
                                                       group baseQ by new { baseQ.EnumFeature, baseQ.CreatedUserId } into g
                                                       select new FeatureAccessTimeModel
                                                       {
                                                           CreatedUserId = g.Key.CreatedUserId,
                                                           EnumFeature = g.Key.EnumFeature,
                                                           AccessTime = g.Sum(x => x.AccessTime),
                                                           Visit = g.Sum(x => x.Visit),
                                                           LastVisited = g.Select(x => x.LastVisited).OrderByDescending(x => x).FirstOrDefault(),
                                                       }).ToListAsync(cancellationToken);

            var featureAccessTimeRequestFeatures = request.FeatureAccessTimes.Where(x => x.EnumFeature.HasValue && x.CourseId.HasValue)
                .Select(f => new
                {
                    f.UserId,
                    CourseId = f.CourseId.GetValueOrDefault(),
                    EnumFeature = f.EnumFeature.GetValueOrDefault(),
                }).ToList();

            var userRequestIds = featureAccessTimeRequestFeatures.Select(x => x.UserId).Distinct().ToList();
            var featureRequests = featureAccessTimeRequestFeatures.Select(x => x.EnumFeature).Distinct().ToList();
            var courseIdRequests = featureAccessTimeRequestFeatures.Select(x => x.CourseId).Distinct().ToList();

            var featureAccessTimeFeatures = await (from baseQ in _featureAccessTimeRepository.Queryable.WhereBulkContains(courseIdRequests, x => x.CourseId)
                                                   .Where(x => featureRequests.Contains(x.EnumFeature))
                                                   .WhereBulkContains(userRequestIds, x => x.CreatedUserId)
                                                   group baseQ by new { baseQ.EnumFeature, baseQ.CourseId, baseQ.CreatedUserId } into g
                                                   select new FeatureAccessTimeModel
                                                   {
                                                       CreatedUserId = g.Key.CreatedUserId,
                                                       EnumFeature = g.Key.EnumFeature,
                                                       CourseId = g.Key.CourseId,
                                                       AccessTime = g.Sum(x => x.AccessTime),
                                                       Visit = g.Sum(x => x.Visit),
                                                       LastVisited = g.Select(x => x.LastVisited).OrderByDescending(x => x).FirstOrDefault(),
                                                   }).ToListAsync(cancellationToken);

            var featureAccessTimeCourseRequests = request.FeatureAccessTimes.Where(x => !x.EnumFeature.HasValue && x.CourseId.HasValue).ToList();

            var userRequesteCourseIds = featureAccessTimeCourseRequests.Select(x => x.UserId).ToList();
            var courseIdRequesteCourses = featureAccessTimeCourseRequests.Select(x => x.EnumFeature).ToList();

            var featureAccessTimeCourses = await (from baseQ in _featureAccessTimeRepository.Queryable.WhereBulkContains(courseIdRequesteCourses, x => x.CourseId)
                                                                                                      .WhereBulkContains(userRequesteCourseIds, x => x.CreatedUserId)
                                                  where baseQ.EnumFeature != Shared.Enums.EnumFeature.Other && baseQ.CourseId.HasValue
                                                  group baseQ by new { baseQ.CourseId, baseQ.CreatedUserId } into g
                                                  select new FeatureAccessTimeModel
                                                  {
                                                      CreatedUserId = g.Key.CreatedUserId,
                                                      CourseId = g.Key.CourseId,
                                                      AccessTime = g.Sum(x => x.AccessTime),
                                                      Visit = g.Sum(x => x.Visit),
                                                      LastVisited = g.Select(x => x.LastVisited).OrderByDescending(x => x).FirstOrDefault(),
                                                  }).ToListAsync(cancellationToken);

            var featureAccessTimeRequests = request.FeatureAccessTimes.Where(x => !x.EnumFeature.HasValue && !x.CourseId.HasValue).ToList();

            var userIds = featureAccessTimeRequests.Select(x => x.UserId).ToList();

            var featureAccessTimes = await (from baseQ in _featureAccessTimeRepository.Queryable.WhereBulkContains(userIds, x => x.CreatedUserId)
                                            group baseQ by new { baseQ.CreatedUserId } into g
                                            select new FeatureAccessTimeModel
                                            {
                                                CreatedUserId = g.Key.CreatedUserId,
                                                AccessTime = g.Sum(x => x.AccessTime),
                                                Visit = g.Sum(x => x.Visit),
                                                LastVisited = g.Select(x => x.LastVisited).OrderByDescending(x => x).FirstOrDefault(),
                                                LearnLastVisited = g.Where(x => x.CourseId.HasValue).Select(x => x.LastVisited).OrderByDescending(x => x).FirstOrDefault(),
                                            }).ToListAsync(cancellationToken);

            featureAccessTimeFeatures.AddRange(featureAccessTimeCourses ?? new List<FeatureAccessTimeModel>());
            featureAccessTimeFeatures.AddRange(featureAccessTimes ?? new List<FeatureAccessTimeModel>());
            featureAccessTimeFeatures.AddRange(featureAccessTimeEnumFeatures ?? new List<FeatureAccessTimeModel>());
            methodResult.Result = featureAccessTimeFeatures;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
