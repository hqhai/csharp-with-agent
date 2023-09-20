// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Queries.FeatureAccessTimeQuery
{
    using Fsel.Common.ActionResults;
    using Fsel.System.Domain.IRepositories;
    using Fsel.System.Domain.Models.EntityModels;
    using Fsel.System.Domain.Models.QueryModels;
    using global::System;
    using global::System.Linq;
    using global::System.Threading.Tasks;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetFeatureAccessTimeQuery : GetFeatureAccessTimeQueryModel, IRequest<MethodResult<FeatureAccessTimeModel>>
    {
    }

    public class GetFeatureAccessTimeQueryHandler : IRequestHandler<GetFeatureAccessTimeQuery, MethodResult<FeatureAccessTimeModel>>
    {
        private readonly IFeatureAccessTimeRepository _featureAccessTimeRepository;

        public GetFeatureAccessTimeQueryHandler(IFeatureAccessTimeRepository featureAccessTimeRepository)
        {
            _featureAccessTimeRepository = featureAccessTimeRepository;
        }

        public async Task<MethodResult<FeatureAccessTimeModel>> Handle(GetFeatureAccessTimeQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<FeatureAccessTimeModel> methodResult = new MethodResult<FeatureAccessTimeModel>();
            FeatureAccessTimeModel? featureAccessTime;
            var query = _featureAccessTimeRepository.Queryable.Where(x => x.CreatedUserId == request.UserId && x.CourseId == request.CourseId);

            if (request.UnitId != null)
            {
                query = query.Where(x => x.UnitId == request.UnitId);
            }

            if (request.LessonId != null)
            {
                query = query.Where(x => x.LessonId == request.LessonId);
            }

            if (request.ObjectId != null)
            {
                query = query.Where(x => x.ObjectId == request.ObjectId);
            }

            if (request.EnumFeature != null)
            {
                query = query.Where(x => x.EnumFeature == request.EnumFeature);
            }

            if (request.LessonId != null)
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
            else if (request.UnitId != null)
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
            methodResult.Result = featureAccessTime;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
