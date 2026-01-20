// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Application.Queries.AiPromptManagerQuery
{
    using System.Collections.Immutable;
    using Common.ActionResults;
    using Domain.Enums;
    using Domain.IRepositories;
    using Domain.Models.CommandModels.AiCriteriaConfig;
    using Domain.Models.QueryModels.AiPromptConfig;
    using Fsel.Common.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Http;
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
                    FeatureMultiple = c.FeatureMultiple.Value,
                    SubFeatureType = c.SubFeatureType.Value
                })
                .Distinct()
                .ToListAsync(cancellationToken);

            // Get all features from enum
            var allFeatures = Enum.GetValues<EnumFeatureMultiple>();

            // Build tree: use DB data, fallback to FeatureModel static config
            var features = allFeatures
                .Select(feature =>
                {
                    // Get subfeatures from DB for this feature
                    var featureDbData = dbData.Where(x => x.FeatureMultiple == feature).ToList();
                    IReadOnlyList<EnumSubFeatureType> subFeatures;

                    if (featureDbData.Any())
                    {
                        // Use DB data
                        subFeatures = featureDbData
                            .Select(x => x.SubFeatureType)
                            .Distinct()
                            .OrderBy(sf => sf)
                            .ToList()
                            .AsReadOnly();
                    }
                    else
                    {
                        // Fallback to FeatureModel static config
                        subFeatures = FeatureModel.FeatureSubFeatures.TryGetValue(feature, out var configSubFeatures)
                            ? configSubFeatures
                            : ImmutableList<EnumSubFeatureType>.Empty;
                    }

                    var typeItems = subFeatures
                        .Select(sf => new GetTypeFeatureQuery(
                            Id: (int)sf,
                            Key: sf.ToString(),
                            Label: sf.DisplayName(),
                            SubFeatures: null
                        ))
                        .ToList()
                        .AsReadOnly();

                    return new GetFeatureAiModelQuery(
                        Id: (int)feature,
                        Key: feature.ToString(),
                        Label: feature.DisplayName(),
                        SubFeatures: typeItems
                    );
                })
                .OrderBy(f => f.Key)
                .ToList()
                .AsReadOnly();

            methodResult.StatusCode = StatusCodes.Status200OK;
            methodResult.Result = features;
            return methodResult;
        }
    }
}
