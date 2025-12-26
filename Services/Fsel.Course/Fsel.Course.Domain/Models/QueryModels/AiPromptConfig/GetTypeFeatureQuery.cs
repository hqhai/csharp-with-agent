// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.QueryModels.AiPromptConfig
{

    public record GetTypeFeatureQuery
    (
        int Id, string Key, string Label,  IEnumerable<GetFeatureCriteriaQuery>? SubFeatures
    );

    public record GetFeatureCriteriaQuery(int Id, string Key, string Label);
}
