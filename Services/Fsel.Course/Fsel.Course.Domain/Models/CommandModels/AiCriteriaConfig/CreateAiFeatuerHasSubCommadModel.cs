// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.CommandModels.AiCriteriaConfig
{
    using Fsel.Course.Domain.Enums;

    public class CreateAiFeatuerHasSubCommadModel
    {

        public EnumCriteriaAi TypeFeatureAi { get; set; }
        public string? UserRole { get; set; }
        public string? Config { get; set; }
        public object? JsonConfig { get; set; }
    }
}
