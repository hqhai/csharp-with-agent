// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Application.Queries.AiPromptManagerQuery
{
    using Common.ActionResults;
    using Domain.Enums;
    using Domain.Models.CommandModels.AiCriteriaConfig;
    using Domain.Models.QueryModels.AiPromptConfig;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Shared.Helpers;

    public class GetFeatureQuery : IRequest<MethodResult<IReadOnlyList<GetFeatureAiModelQuery>>>
    {
    }

    public class GetFeatureQueryHandler : IRequestHandler<GetFeatureQuery, MethodResult<IReadOnlyList<GetFeatureAiModelQuery>>>
    {
        public Task<MethodResult<IReadOnlyList<GetFeatureAiModelQuery>>> Handle(GetFeatureQuery request, CancellationToken cancellationToken)
        {
            var methodResult = new MethodResult<IReadOnlyList<GetFeatureAiModelQuery>>();

            var featureTypeLookup = FeatureModel.FeatureTypes
                .GroupBy(x => x.Feature)
                .ToDictionary(g => g.Key, g => g.SelectMany(x => x.SubFeatures).ToList());

            var criteriaLookup = FeatureModel.FeatureCriteria
                .ToDictionary(x => x.SubFeature, x => x.Criterias);

            var features = Enum.GetValues<EnumFeatureMultiple>()
                .Select(feature =>
                {
                    var subTypes = featureTypeLookup.TryGetValue(feature, out var subs)
                        ? subs
                        : new List<EnumSubFeatureType>();

                    var typeItems = subTypes
                        .Select(sub =>
                        {
                            var criteriaEnums = criteriaLookup.TryGetValue(sub, out var crits)
                                ? crits
                                : new List<EnumCriteriaAi>();

                            var criteriaItems = criteriaEnums
                                .Select(c => new GetFeatureCriteriaQuery(
                                    Id: (int)c,
                                    Key: c.ToString(),
                                    Label: c.DisplayName()
                                ))
                                .ToList()
                                .AsReadOnly();

                            return new GetTypeFeatureQuery(
                                Id: (int)sub,
                                Key: sub.ToString(),
                                Label: sub.DisplayName(),
                                SubFeatures: criteriaItems
                            );
                        })
                        .ToList()
                        .AsReadOnly();

                    return new GetFeatureAiModelQuery(
                        Id: (int)feature,
                        Key: feature.ToString(),
                        Label: feature.DisplayName(),
                        SubFeatures: typeItems
                    );
                })
                .OrderBy(x => x.Key)
                .ToList()
                .AsReadOnly();

            methodResult.StatusCode = StatusCodes.Status200OK;
            methodResult.Result = features;
            return Task.FromResult(methodResult);
        }
    }
}
