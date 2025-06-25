// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels.SkillModels
{
    using Fsel.Core.Base.BaseModels;
    using Fsel.Course.Domain.Entities;

    public class SkillModel : BaseModel
    {
        public string? FilePath { get; set; }
        public string? Name { get; set; }
        public string? Code { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; }
    }
}
