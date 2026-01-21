// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Application.Queries.AiPromptManagerQuery
{
    using System.Collections.Immutable;
    using System.Linq;
    using Common.ActionResults;
    using Domain.Enums;
    using Domain.IRepositories;
    using Domain.Models.CommandModels.AiCriteriaConfig;
    using Domain.Models.QueryModels.AiPromptConfig;
    using Fsel.Common.Enums;
    using MediatR;
    using Microsoft.EntityFrameworkCore;
    using Shared.Helpers;

    public class GetFeatureQuery : IRequest<MethodResult<IReadOnlyList<GetFeatureAiModelQuery>>>
    {
    }

    public class GetFeatureQueryHandler : IRequestHandler<GetFeatureQuery, MethodResult<IReadOnlyList<GetFeatureAiModelQuery>>>
    {
        private readonly IAiCriteriaConfigRepository _aiCriteriaConfigRepository;

        public GetFeatureQueryHandler(IAiCriteriaConfigRepository aiCriteriaConfigRepository)
        {
            _aiCriteriaConfigRepository = aiCriteriaConfigRepository;
        }

        public async Task<MethodResult<IReadOnlyList<GetFeatureAiModelQuery>>> Handle(GetFeatureQuery request, CancellationToken cancellationToken)
        {
            var methodResult = new MethodResult<IReadOnlyList<GetFeatureAiModelQuery>>();

            // Get all distinct FeatureMultiple and SubFeatureType pairs from DB
            var dbData = await _aiCriteriaConfigRepository.Queryable
                .Where(c => c.FeatureMultiple.HasValue
                            && c.SubFeatureType.HasValue
                            && c.VersionStatus == EnumVersionStatus.LastVersion)
                .Select(c => new
                {
                    FeatureMultiple = c.FeatureMultiple!.Value,
                    SubFeatureType = c.SubFeatureType!.Value
                })
                .Distinct()
                .OrderBy(a => a.FeatureMultiple)
                .ThenBy(a => a.SubFeatureType)
                .ToListAsync(cancellationToken);

            // Get all features from enum
            var allFeatures = Enum.GetValues<EnumFeatureMultiple>();


            var features = BuildFeatureTree(
                allFeatures,
                dbData.Select(x => (x.FeatureMultiple, x.SubFeatureType))
            );

            methodResult.Result = features;
            return methodResult;
        }

        private static IReadOnlyList<GetFeatureCriteriaQuery> BuildCriteriaItems(EnumSubFeatureType subFeature)
        {
            var criterias = FeatureModel.SubFeatureCriteria.TryGetValue(subFeature, out var list)
                ? list
                : ImmutableList<EnumCriteriaAi>.Empty;

            return criterias
                .Select(c => new GetFeatureCriteriaQuery(
                    Id: (int)c,
                    Key: c.ToString(),
                    Label: c.DisplayName()
                ))
                .ToList()
                .AsReadOnly();
        }

        private static IReadOnlyList<GetTypeFeatureQuery> BuildSubFeatureItems(
            EnumFeatureMultiple feature,
            IEnumerable<(EnumFeatureMultiple Feature, EnumSubFeatureType SubFeature)> dbData)
        {
            var dbSubFeatures = dbData
                .Where(x => x.Feature == feature)
                .Select(x => x.SubFeature);

            var configSubFeatures =
                FeatureModel.FeatureSubFeatures.TryGetValue(feature, out var config)
                    ? config
                    : Enumerable.Empty<EnumSubFeatureType>();

            return dbSubFeatures
                .Union(configSubFeatures)
                .OrderBy(sf => sf)
                .Select(sf => new GetTypeFeatureQuery(
                    Id: (int)sf,
                    Key: sf.ToString(),
                    Label: sf.DisplayName(),
                    SubFeatures: BuildCriteriaItems(sf)
                ))
                .ToList()
                .AsReadOnly();
        }

        private static IReadOnlyList<GetFeatureAiModelQuery> BuildFeatureTree(
            IEnumerable<EnumFeatureMultiple> allFeatures,
            IEnumerable<(EnumFeatureMultiple Feature, EnumSubFeatureType SubFeature)> dbData)
        {
            return allFeatures
                .Select(feature => new GetFeatureAiModelQuery(
                    Id: (int)feature,
                    Key: feature.ToString(),
                    Label: feature.DisplayName(),
                    SubFeatures: BuildSubFeatureItems(feature, dbData)
                ))
                .OrderBy(f => f.Key)
                .ToList()
                .AsReadOnly();
        }

    }
}
