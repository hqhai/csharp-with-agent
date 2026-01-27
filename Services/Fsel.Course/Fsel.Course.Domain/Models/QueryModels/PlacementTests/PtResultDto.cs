// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.QueryModels.PlacementTests
{
    using System;
    using System.Collections.Generic;

    public class PtResultDto
    {
        public Guid Id { get; set; }
        public string SubjectName { get; set; }

        public string CurrentLevel { get; set; }

        public string SuggestLevel { get; set; }

        public List<ModuleResult> ModuleResults { get; set; }
    }

    public class ModuleResult
    {
        public string ModuleName { get; set; }

        public double Percent { get; set; }

        public string LevelOfModule { get; set; }

        public List<SkillResult> SkillResults { get; set; }
    }

    public class SkillResult
    {
        public double Percent { get; set; }

        public string SkillName { get; set; }
    }
}
