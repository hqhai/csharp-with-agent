// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.QueryModels.AiPromptConfig
{
    public record GetFeatureAiModelQuery
    (
        int Id, string Key, string Label, IEnumerable<GetTypeFeatureQuery>? SubFeatures
    );
}
