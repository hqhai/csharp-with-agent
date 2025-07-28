// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Entities
{
    using Fsel.Core.Entities;

    public class SkillLevel : Entity
    {
        public Guid LevelId { get; set; }

        public Guid SkillId { get; set; }

        public Skill? Skill { get; set; }

        public Level? Level { get; set; }
    }
}
