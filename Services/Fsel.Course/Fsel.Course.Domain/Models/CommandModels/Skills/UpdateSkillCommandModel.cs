// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.CommandModels.Skills
{
    using Fsel.Core.Base.BaseModels;

    public class UpdateSkillCommandModel : BaseCommandModel
    {
        public string? FilePath { get; set; }
        public string? Name { get; set; }
        public string? Code { get; set; }
        public string? Description { get; set; }
    }
}
