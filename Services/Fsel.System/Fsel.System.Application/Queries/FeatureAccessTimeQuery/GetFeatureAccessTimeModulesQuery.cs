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

    public class GetFeatureAccessTimeModulesQuery : GetFeatureAccessTimesQueryModel, IRequest<MethodResult<IList<FeatureAccessTimeModel>>>
    {
    }

    public class GetFeatureAccessTimeModulesQueryHandler : IRequestHandler<GetFeatureAccessTimeModulesQuery, MethodResult<IList<FeatureAccessTimeModel>>>
    {
        private readonly IFeatureAccessTimeRepository _featureAccessTimeRepository;

        public GetFeatureAccessTimeModulesQueryHandler(IFeatureAccessTimeRepository featureAccessTimeRepository)
        {
            _featureAccessTimeRepository = featureAccessTimeRepository;
        }

        public async Task<MethodResult<IList<FeatureAccessTimeModel>>> Handle(GetFeatureAccessTimeModulesQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<IList<FeatureAccessTimeModel>> methodResult = new MethodResult<IList<FeatureAccessTimeModel>>();
            var featureAccessTimes = new List<FeatureAccessTimeModel>();
            foreach (var item in request.FeatureAccessTimes)
            {
                FeatureAccessTimeModel? featureAccessTime;
                var userId = request.UserId == Guid.Empty ? item.UserId : request.UserId;
                var query = _featureAccessTimeRepository.Queryable.Where(x => x.CreatedUserId == userId && (!item.CourseId.HasValue || x.CourseId == item.CourseId) && x.ObjectId.HasValue);

                if (item.UnitId.HasValue)
                {
                    query = query.Where(x => x.UnitId == item.UnitId);
                }
                if (item.LessonId.HasValue)
                {
                    query = query.Where(x => x.LessonId == item.LessonId);
                }
                if (item.ObjectId.HasValue)
                {
                    query = query.Where(x => x.ObjectId == item.ObjectId);
                }
                if (item.EnumFeature.HasValue)
                {
                    query = query.Where(x => x.EnumFeature == item.EnumFeature);
                }
                if (item.EnumFeature.HasValue && !item.CourseId.HasValue)
                {
                    featureAccessTime = await query.GroupBy(x => x.EnumFeature).Select(x => new FeatureAccessTimeModel
                    {
                        AccessTime = x.Sum(x => x.AccessTime),
                        UserId = userId,
                        Visit = x.Sum(x => x.Visit),
                        LastVisited = x.Select(x => x.LastVisited).OrderByDescending(x => x).FirstOrDefault(),
                        EnumFeature = x.Key,
                    }).FirstOrDefaultAsync(cancellationToken);
                }
                else if (item.EnumFeature.HasValue && item.CourseId.HasValue)
                {
                    featureAccessTime = await query.GroupBy(x => new { x.EnumFeature, x.CourseId }).Select(x => new FeatureAccessTimeModel
                    {
                        AccessTime = x.Sum(x => x.AccessTime),
                        Visit = x.Sum(x => x.Visit),
                        UserId = userId,
                        LastVisited = x.Select(x => x.LastVisited).OrderByDescending(x => x).FirstOrDefault(),
                        EnumFeature = x.Key.EnumFeature,
                        CourseId = x.Key.CourseId,
                    }).FirstOrDefaultAsync(cancellationToken);
                }
                else if (item.CourseId.HasValue)
                {
                    featureAccessTime = await query.Where(x => x.CourseId.HasValue).GroupBy(x => x.CourseId).Select(x => new FeatureAccessTimeModel
                    {
                        AccessTime = x.Sum(x => x.AccessTime),
                        Visit = x.Sum(x => x.Visit),
                        UserId = userId,
                        LastVisited = x.Select(x => x.LastVisited).OrderByDescending(x => x).FirstOrDefault(),
                        CourseId = x.Key,
                    }).FirstOrDefaultAsync(cancellationToken);
                }
                else
                {
                    featureAccessTime = await query.OrderByDescending(x => x.LastVisited).Select(x => new FeatureAccessTimeModel
                    {
                        AccessTime = x.AccessTime,
                        Visit = x.Visit,
                        UserId = userId,
                        LastVisited = x.LastVisited
                    }).FirstOrDefaultAsync(cancellationToken);
                }
                featureAccessTimes.Add(featureAccessTime ?? new FeatureAccessTimeModel());
            }
            methodResult.Result = featureAccessTimes;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
