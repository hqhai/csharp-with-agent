// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    using Fsel.Core.Base.BaseModels;

    public class LevelModel : BaseModel
    {
        public string? Name { get; set; }

        public string? ProgramLevelName { get; set; }

        public string? ProgramDescription { get; set; }

        public string? Code { get; set; }

        public string? Description { get; set; }

        public int LevelOrder { get; set; }

        public bool LearnedBefore { get; set; }

        public IList<SkillViewModel>? Skils { get; set; }

    }

    public class SelectionLevelModel : LevelModel
    {
        public bool CanSelect { get; set; }

        public Guid ProgramId { get; set; }

        public string? CourseType { get; set; }

        public bool IsCurrentLevel { get; set; }

        public bool IsAvailableCourse { get; set; }
    }
}
