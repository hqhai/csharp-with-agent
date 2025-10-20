// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels.AiManagerModels
{
    using Fsel.Core.Base.BaseModels;
    using Fsel.Course.Domain.Enums;
    using Fsel.Shared.Enums;

    public class AiFeatureModel : BaseModel
    {
        public Guid FeatureObjectId { get; set; }
        public Guid? ParentFeatureId { get; set; }
        public Guid AiModelManagerId { get; set; }
        public EnumFeatureAi FeatureAi { get; set; }
        public EnumTypeFeatureAi? TypeFeatureAi { get; set; }
        public string? UserRole { get; set; }
        public string? Config { get; set; }
        public string? Json { get; set; }
        public double? SettingTemperature { get; set; }
        public double? SettingWordMaxLength { get; set; }
        public double? SettingTopP { get; set; }
        public double? SettingFrequency { get; set; }
        public double? SettingPresence { get; set; }
        public int? MaximumNumber { get; set; }
        public int? MaximumToken { get; set; }
        public List<AiSubFeatueModel>? SubFeatures { get; set; }
    }
}
