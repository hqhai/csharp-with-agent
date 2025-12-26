// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.QueryModels.AiModelManager
{
    using Fsel.Core.Base.BaseModels;

    public class SearchAiModelManagerQueryModel : BaseQueryModel
    {
        public string? AiModelName { get; set; }
    }
}
