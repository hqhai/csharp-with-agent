// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Queries.FeatureAccessTimeQuery
{
    using Fsel.Common.ActionResults;
    using Fsel.Shared.Enums;
    using Fsel.System.Domain.IRepositories;
    using Fsel.System.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetFeatureAccessTimesByTestQuery : IRequest<MethodResult<FeatureAccessTimeCourseModel>>
    {
        public Guid CourseId { get; set; }
        public Guid UserId { get; set; }
        public Guid ObjectId { get; set; }
        public EnumFeature EnumFeature { get; set; }
    }

    public class GetFeatureAccessTimesByTestQueryHandler : IRequestHandler<GetFeatureAccessTimesByTestQuery, MethodResult<FeatureAccessTimeCourseModel>>
    {
        private readonly IFeatureAccessTimeRepository _featureAccessTimeRepository;

        public GetFeatureAccessTimesByTestQueryHandler(IFeatureAccessTimeRepository featureAccessTimeRepository)
        {
            _featureAccessTimeRepository = featureAccessTimeRepository;
        }

        public async Task<MethodResult<FeatureAccessTimeCourseModel>> Handle(GetFeatureAccessTimesByTestQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<FeatureAccessTimeCourseModel> methodResult = new MethodResult<FeatureAccessTimeCourseModel>();

            var featureAccessTime = await _featureAccessTimeRepository.Queryable.Where(x => x.CreatedUserId == request.UserId && x.CourseId == request.CourseId)
                .Where(x => x.ObjectId == request.ObjectId && x.EnumFeature == request.EnumFeature)
                .Select(x => new FeatureAccessTimeCourseModel
                {
                    CourseId = x.CourseId,
                    AccessTime = x.AccessTime,
                    TotalVisit = x.Visit,
                    LastVisited = x.LastVisited ?? default
                }).FirstOrDefaultAsync(cancellationToken);
            methodResult.Result = featureAccessTime;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
