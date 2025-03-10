// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Queries.FeatureAccessTimeQuery
{
    using Fsel.Common.ActionResults;
    using Fsel.Shared.Enums;
    using Fsel.System.Domain.IRepositories;
    using Fsel.System.Domain.Models.EntityModels;
    using Fsel.System.Domain.Models.QueryModels;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class GetFeatureAccessTimeBusinessQuery : GetFeatureAccessTimeBusinessQueryModel, IRequest<MethodResult<IList<FeatureAccessTimeBusinessModel>>>
    {
    }

    public class GetFeatureAccessTimeBusinessQueryHandler : IRequestHandler<GetFeatureAccessTimeBusinessQuery, MethodResult<IList<FeatureAccessTimeBusinessModel>>>
    {
        private readonly IFeatureAccessTimeRepository _featureAccessTimeRepository;

        public GetFeatureAccessTimeBusinessQueryHandler(IFeatureAccessTimeRepository featureAccessTimeRepository)
        {
            _featureAccessTimeRepository = featureAccessTimeRepository;
        }

        public async Task<MethodResult<IList<FeatureAccessTimeBusinessModel>>> Handle(GetFeatureAccessTimeBusinessQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<IList<FeatureAccessTimeBusinessModel>>();

            var featureAccessTime = await _featureAccessTimeRepository.Queryable.Where(p => p.CreatedUserId == request.UserId)
                                                                                .Where(x => !request.CourseId.HasValue || x.CourseId == request.CourseId)
                                                                                .ToListAsync(cancellationToken);
            if (request.StartDate.HasValue && request.EndDate.HasValue)
            {
                featureAccessTime = featureAccessTime.Where(p => (p.UpdatedDate ?? p.CreatedDate) >= request.StartDate && (p.UpdatedDate ?? p.CreatedDate) <= request.EndDate).ToList();
            }
            if (request.StartDate.HasValue)
            {
                featureAccessTime = featureAccessTime.Where(p => (p.UpdatedDate ?? p.CreatedDate) >= request.StartDate).ToList();
            }
            if (request.EndDate.HasValue)
            {
                featureAccessTime = featureAccessTime.Where(p => (p.UpdatedDate ?? p.CreatedDate) <= request.EndDate).ToList();
            }

            var learn = featureAccessTime.Where(p => p.EnumFeature == EnumFeature.HomeWork || p.EnumFeature == EnumFeature.VideoLesson || p.EnumFeature == EnumFeature.MockTest || p.EnumFeature == EnumFeature.FinalTest).ToList();

            var social = featureAccessTime.Where(p => p.EnumFeature == EnumFeature.ClassForum || p.EnumFeature == EnumFeature.DiscussionBoard).ToList();

            var other = featureAccessTime.Where(p => p.EnumFeature == EnumFeature.Other).ToList();

            methodResult.Result = new List<FeatureAccessTimeBusinessModel>()
            {
                new FeatureAccessTimeBusinessModel()
                {
                    FeatureBusinessType = EnumFeatureBussinessType.Learn,
                    TotalVisit = learn.Sum(p => p.Visit),
                    AccessTime = learn.Sum(p => p.AccessTime),
                },
                new FeatureAccessTimeBusinessModel()
                {
                    FeatureBusinessType = EnumFeatureBussinessType.Social,
                    TotalVisit = social.Sum(p => p.Visit),
                    AccessTime = social.Sum(p => p.AccessTime),
                },
                new FeatureAccessTimeBusinessModel()
                {
                    FeatureBusinessType = EnumFeatureBussinessType.Other,
                    TotalVisit = other.Sum(p => p.Visit),
                    AccessTime = other.Sum(p => p.AccessTime),
                }
            };
            return methodResult;
        }
    }
}
