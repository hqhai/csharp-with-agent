// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels.SkillModels
{
    using Fsel.Core.Base.BaseModels;

    public class SkillModel : BaseModel
    {
        public string? FilePath { get; set; }
        public string? Name { get; set; }
        public string? Code { get; set; }
        public string? Description { get; set; }
        public string? ColorCode { get; set; }
        public bool IsActive { get; set; }
    }
}
