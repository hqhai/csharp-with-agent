// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Application.Queries.AiPromptManagerQuery
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Domain.Enums;
    using Domain.Models.QueryModels.AiPromptConfig;
    using Fsel.Common.ActionResults;
    using Fsel.Course.Domain.Models.CommandModels.AiCriteriaConfig;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Helpers;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class GetFeatureQuery : IRequest<MethodResult<IReadOnlyList<GetFeatureAiModelQuery>>>
    {
    }

    public class GetFeatureQueryHandler : IRequestHandler<GetFeatureQuery, MethodResult<IReadOnlyList<GetFeatureAiModelQuery>>>
    {
        public Task<MethodResult<IReadOnlyList<GetFeatureAiModelQuery>>> Handle(GetFeatureQuery request, CancellationToken cancellationToken)
        {
            var methodResult = new MethodResult<IReadOnlyList<GetFeatureAiModelQuery>>();

            var features = Enum.GetValues<EnumFeatureMultiple>()
                .Select(feature =>
                {
                    var subTypes = FeatureModel.FeatureTypes.TryGetValue(feature, out var subArray)
                        ? subArray
                        : Array.Empty<EnumSubFeatureType>();

                    var typeItems = subTypes
                        .Select(sub =>
                        {
                            var criteriaEnums = FeatureModel.FeatureCriteria.TryGetValue(sub, out var crits)
                                ? crits
                                : Array.Empty<EnumCriteriaAi>();

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
