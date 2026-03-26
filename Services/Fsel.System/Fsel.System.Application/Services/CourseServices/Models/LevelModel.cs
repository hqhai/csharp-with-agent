// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Services.CourseServices.Models
{
    using Fsel.Core.Base.BaseModels;

    public class LevelModel : BaseModel
    {
        public string? Name { get; set; }

        public string? ProgramLevelName { get; set; }

        public string? ProgramDescription { get; set; }

        public string? Code { get; set; }

        public string? Description { get; set; }

        public string? PTDescription { get; set; }

        public int LevelOrder { get; set; }

        public bool LearnedBefore { get; set; }
    }
}
