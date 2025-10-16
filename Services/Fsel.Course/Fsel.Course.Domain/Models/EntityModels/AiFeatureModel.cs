// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    using Core.Base.BaseModels;

    public class AiFeatureModel
    {
        public Guid Id { get; set; }
        public Guid AiPromptConfigId { get; set; }
        public string? UserRole { get; set; }
        public string? Config { get; set; }
        public string? Json { get; set; }
        public int? MaximumNumber { get; set; }
        public int? MaximumToken { get; set; }
    }
}
