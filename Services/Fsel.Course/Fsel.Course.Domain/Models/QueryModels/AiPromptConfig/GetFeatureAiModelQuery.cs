// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.QueryModels.AiModelFeature
{
    public record GetFeatureAiModelQuery
    (
        int Id, string Key, string Label, string Group, int Order, IEnumerable<GetTypeFeatureQuery>? Types
    );
}
