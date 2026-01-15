// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.CommandModels.Skills
{
    public class CreateSkillCommandModel
    {
        public string? Name { get; set; }
        public string? Code { get; set; }
        public string? FilePath { get; set; }
        public string? Description { get; set; }
        public string? ColorCode { get; set; }
    }
}
