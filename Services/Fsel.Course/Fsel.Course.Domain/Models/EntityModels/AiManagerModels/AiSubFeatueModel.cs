// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels.AiManagerModels
{
    using Fsel.Core.Base.BaseModels;
    using Fsel.Course.Domain.Enums;

    public class AiSubFeatueModel : BaseModel
    {
        public EnumTypeFeatureAi TypeFeatureAi { get; set; }
        public string? UserRole { get; set; }
        public string? Config { get; set; }
        public string? Json { get; set; }
        public SetiingAiFeatureModel? Setting { get; set; }

        public AiSubFeatueModel(EnumTypeFeatureAi item, string? userRole, string? config, string? json, SetiingAiFeatureModel subSettings)
        {
            TypeFeatureAi = item;
            UserRole = userRole;
            Config = config;
            Json = json;
            Setting = subSettings;
        }

        public AiSubFeatueModel() { }
    }
}
