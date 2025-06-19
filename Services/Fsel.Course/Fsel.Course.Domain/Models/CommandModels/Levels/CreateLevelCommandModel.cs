// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.CommandModels.Levels
{
    public class CreateLevelCommandModel
    {
        public string? Name { get; set; }

        public string? Code { get; set; }

        public string? Description { get; set; }

        public int LevelOrder { get; set; }

        public Guid ProgramId { get; set; }

        public IList<Guid>? SkillIds { get; set; }
    }
}
