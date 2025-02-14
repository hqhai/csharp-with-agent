// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Queries.FeatureAccessTimeQuery
{
    using Fsel.Common.ActionResults;
    using Fsel.Shared.Models.ShareModels.QueryModels;
    using Fsel.System.Domain.IRepositories;
    using Fsel.System.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;

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

            var featureAccessTimeEnumFeatures = (from baseQ in _featureAccessTimeRepository.Queryable.AsEnumerable()
                                                 join fe in featureAccessTimeRequestEnumFeatures.AsEnumerable() on new { CreatedUserId = baseQ.CreatedUserId, EnumFeature = baseQ.EnumFeature }
                                                 equals new { CreatedUserId = fe.UserId, fe.EnumFeature }
                                                 group baseQ by new { baseQ.EnumFeature, baseQ.CreatedUserId } into g
                                                 select new FeatureAccessTimeModel
                                                 {
                                                     CreatedUserId = g.Key.CreatedUserId,
                                                     EnumFeature = g.Key.EnumFeature,
                                                     AccessTime = g.Sum(x => x.AccessTime),
                                                     Visit = g.Sum(x => x.Visit),
                                                     LastVisited = g.Select(x => x.LastVisited).OrderByDescending(x => x).FirstOrDefault(),
                                                 }).ToList();

            var featureAccessTimeRequestFeatures = request.FeatureAccessTimes.Where(x => x.EnumFeature.HasValue && x.CourseId.HasValue)
                .Select(f => new
                {
                    f.UserId,
                    CourseId = f.CourseId.GetValueOrDefault(),
                    EnumFeature = f.EnumFeature.GetValueOrDefault(),
                }).ToList();

            var featureAccessTimeFeatures = (from baseQ in _featureAccessTimeRepository.Queryable.AsEnumerable()
                                             join fe in featureAccessTimeRequestFeatures.AsEnumerable() on new { CreatedUserId = baseQ.CreatedUserId, CourseId = baseQ.CourseId, EnumFeature = baseQ.EnumFeature }
                                             equals new { CreatedUserId = fe.UserId, CourseId = (Guid?)fe.CourseId, fe.EnumFeature }
                                             group baseQ by new { baseQ.EnumFeature, baseQ.CourseId, baseQ.CreatedUserId } into g
                                             select new FeatureAccessTimeModel
                                             {
                                                 CreatedUserId = g.Key.CreatedUserId,
                                                 EnumFeature = g.Key.EnumFeature,
                                                 CourseId = g.Key.CourseId,
                                                 AccessTime = g.Sum(x => x.AccessTime),
                                                 Visit = g.Sum(x => x.Visit),
                                                 LastVisited = g.Select(x => x.LastVisited).OrderByDescending(x => x).FirstOrDefault(),
                                             }).ToList();

            var featureAccessTimeCourseRequests = request.FeatureAccessTimes.Where(x => !x.EnumFeature.HasValue && x.CourseId.HasValue).ToList();
            var featureAccessTimeCourses = (from baseQ in _featureAccessTimeRepository.Queryable.AsEnumerable()
                                            join fe in featureAccessTimeCourseRequests.AsEnumerable() on new { CreatedUserId = baseQ.CreatedUserId, CourseId = baseQ.CourseId }
                                            equals new { CreatedUserId = fe.UserId, CourseId = fe.CourseId }
                                            where baseQ.EnumFeature != Shared.Enums.EnumFeature.Other && baseQ.CourseId.HasValue
                                            group baseQ by new { baseQ.CourseId, baseQ.CreatedUserId } into g
                                            select new FeatureAccessTimeModel
                                            {
                                                CreatedUserId = g.Key.CreatedUserId,
                                                CourseId = g.Key.CourseId,
                                                AccessTime = g.Sum(x => x.AccessTime),
                                                Visit = g.Sum(x => x.Visit),
                                                LastVisited = g.Select(x => x.LastVisited).OrderByDescending(x => x).FirstOrDefault(),
                                            }).ToList();

            var featureAccessTimeRequests = request.FeatureAccessTimes.Where(x => !x.EnumFeature.HasValue && !x.CourseId.HasValue).ToList();
            var featureAccessTimes = (from baseQ in _featureAccessTimeRepository.Queryable.AsEnumerable()
                                      join fe in featureAccessTimeRequests.AsEnumerable() on baseQ.CreatedUserId equals fe.UserId
                                      group baseQ by new { baseQ.CreatedUserId } into g
                                      select new FeatureAccessTimeModel
                                      {
                                          CreatedUserId = g.Key.CreatedUserId,
                                          AccessTime = g.Sum(x => x.AccessTime),
                                          Visit = g.Sum(x => x.Visit),
                                          LastVisited = g.Select(x => x.LastVisited).OrderByDescending(x => x).FirstOrDefault(),
                                          LearnLastVisited = g.Where(x => x.CourseId.HasValue).Select(x => x.LastVisited).OrderByDescending(x => x).FirstOrDefault(),
                                      }).ToList();

            featureAccessTimeFeatures.AddRange(featureAccessTimeCourses ?? new List<FeatureAccessTimeModel>());
            featureAccessTimeFeatures.AddRange(featureAccessTimes ?? new List<FeatureAccessTimeModel>());
            featureAccessTimeFeatures.AddRange(featureAccessTimeEnumFeatures ?? new List<FeatureAccessTimeModel>());
            methodResult.Result = featureAccessTimeFeatures;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
