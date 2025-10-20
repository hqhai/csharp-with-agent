// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels.AiManagerModels
{
    using Core.Base.BaseModels;

    public class AiPromptManagerModel : BaseModel
    {
        public string? AiModelName { get; set; }

        public string? InputModel { get; set; }
    }
}
