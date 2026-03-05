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

            var query = _featureAccessTimeRepository.ReadQueryable
                                                    .Where(p => p.CreatedUserId == request.UserId)
                                                    .Where(x => !request.CourseId.HasValue || x.CourseId == request.CourseId);

            if (request.StartDate.HasValue && request.EndDate.HasValue)
            {
                query = query.Where(p => (p.UpdatedDate ?? p.CreatedDate) >= request.StartDate && (p.UpdatedDate ?? p.CreatedDate) <= request.EndDate);
            }
            if (request.StartDate.HasValue)
            {
                query = query.Where(p => (p.UpdatedDate ?? p.CreatedDate) >= request.StartDate);
            }
            if (request.EndDate.HasValue)
            {
                query = query.Where(p => (p.UpdatedDate ?? p.CreatedDate) <= request.EndDate);
            }
            var featureAccessTime = await query.ToListAsync(cancellationToken);
            var learnFeatures = new[]
            {
                EnumFeature.HomeWork,
                EnumFeature.Document,
                EnumFeature.VideoLesson,
                EnumFeature.FullTest,
                EnumFeature.SkillTest,
                EnumFeature.ChatBot
            };

            var socialFeatures = new[]
            {
                EnumFeature.ClassForum,
                EnumFeature.DiscussionBoard
            };

            var learn = featureAccessTime.Where(x => learnFeatures.Contains(x.EnumFeature));
            var social = featureAccessTime.Where(x => socialFeatures.Contains(x.EnumFeature));
            var other = featureAccessTime.Where(x => x.EnumFeature == EnumFeature.Other);

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
