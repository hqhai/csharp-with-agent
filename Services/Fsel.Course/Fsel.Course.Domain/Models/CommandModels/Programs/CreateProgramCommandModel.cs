// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.CommandModels.Programs
{
    using Fsel.Course.Domain.Enums;
    using Fsel.Shared.Enums;

    public class CreateProgramCommandModel
    {
        public string? Thumbnail { get; set; }
        public Guid ParentId { get; set; }

        public string? Name { get; set; }

        public string? Code { get; set; }

        public string? Description { get; set; }

        public EnumStatus Status { get; set; }
        public bool IsTestDefault { get; set; }
        public EnumTestMode? TestMode { get; set; }
        public IList<Guid>? TestOriginalIds { get; set; }
        public IList<UpdateLevelCommandModel>? Levels { get; set; }
    }

    public class UpdateLevelCommandModel
    {
        public Guid? Id { get; set; }

        public string? Name { get; set; }

        public string? Code { get; set; }

        public string? Description { get; set; }

        public int LevelOrder { get; set; }

        public IList<Guid>? SkillIds { get; set; }
    }
}
