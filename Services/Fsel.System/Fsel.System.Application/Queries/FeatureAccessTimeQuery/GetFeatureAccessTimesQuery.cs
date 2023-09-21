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

    public class GetFeatureAccessTimesQuery : GetFeatureAccessTimesQueryModel, IRequest<MethodResult<IList<FeatureAccessTimeModel>>>
    {
    }

    public class GetFeatureAccessTimesQueryHandler : IRequestHandler<GetFeatureAccessTimesQuery, MethodResult<IList<FeatureAccessTimeModel>>>
    {
        private readonly IFeatureAccessTimeRepository _featureAccessTimeRepository;

        public GetFeatureAccessTimesQueryHandler(IFeatureAccessTimeRepository featureAccessTimeRepository)
        {
            _featureAccessTimeRepository = featureAccessTimeRepository;
        }

        public async Task<MethodResult<IList<FeatureAccessTimeModel>>> Handle(GetFeatureAccessTimesQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<IList<FeatureAccessTimeModel>> methodResult = new MethodResult<IList<FeatureAccessTimeModel>>();
            var featureAccessTimes = new List<FeatureAccessTimeModel>();
            foreach (var item in request.FeatureAccessTimes)
            {
                FeatureAccessTimeModel? featureAccessTime;
                var query = _featureAccessTimeRepository.Queryable.Where(x => x.CreatedUserId == request.UserId && x.CourseId == item.CourseId);
                if (item.UnitId != null)
                {
                    query = query.Where(x => x.UnitId == item.UnitId);
                }

                if (item.LessonId != null)
                {
                    query = query.Where(x => x.LessonId == item.LessonId);
                }

                if (item.ObjectId != null)
                {
                    query = query.Where(x => x.ObjectId == item.ObjectId);
                }

                if (item.EnumFeature != null)
                {
                    query = query.Where(x => x.EnumFeature == item.EnumFeature);
                }

                if (item.ObjectId != null)
                {
                    featureAccessTime = await query.GroupBy(x => new { x.CourseId, x.UnitId, x.LessonId, x.ObjectId }).Select(x => new FeatureAccessTimeModel
                    {
                        AccessTime = x.Sum(x => x.AccessTime),
                        Visit = x.Sum(x => x.Visit),
                        LastVisited = x.Select(x => x.LastVisited).OrderByDescending(x => x).FirstOrDefault(),
                        CourseId = x.Key.CourseId,
                        UnitId = x.Key.UnitId,
                        LessonId = x.Key.LessonId,
                        ObjectId = x.Key.ObjectId,
                    }).FirstOrDefaultAsync(cancellationToken);
                }
                else if (item.LessonId != null)
                {
                    featureAccessTime = await query.GroupBy(x => new { x.CourseId, x.UnitId, x.LessonId, x.ObjectId }).Select(x => new FeatureAccessTimeModel
                    {
                        AccessTime = x.Sum(x => x.AccessTime),
                        Visit = x.Sum(x => x.Visit),
                        LastVisited = x.Select(x => x.LastVisited).OrderByDescending(x => x).FirstOrDefault(),
                        CourseId = x.Key.CourseId,
                        UnitId = x.Key.UnitId,
                        LessonId = x.Key.LessonId,
                        ObjectId = x.Key.ObjectId,
                    }).FirstOrDefaultAsync(cancellationToken);
                }
                else if (item.UnitId != null)
                {
                    featureAccessTime = await query.GroupBy(x => new { x.CourseId, x.UnitId, x.ObjectId }).Select(x => new FeatureAccessTimeModel
                    {
                        AccessTime = x.Sum(x => x.AccessTime),
                        Visit = x.Sum(x => x.Visit),
                        LastVisited = x.Select(x => x.LastVisited).OrderByDescending(x => x).FirstOrDefault(),
                        CourseId = x.Key.CourseId,
                        UnitId = x.Key.UnitId,
                        ObjectId = x.Key.ObjectId,
                    }).FirstOrDefaultAsync(cancellationToken);
                }
                else
                {
                    featureAccessTime = await query.GroupBy(x => new { x.CourseId, x.ObjectId }).Select(x => new FeatureAccessTimeModel
                    {
                        AccessTime = x.Sum(x => x.AccessTime),
                        Visit = x.Sum(x => x.Visit),
                        LastVisited = x.Select(x => x.LastVisited).OrderByDescending(x => x).FirstOrDefault(),
                        CourseId = x.Key.CourseId,
                        ObjectId = x.Key.ObjectId,
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
