// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Master.Domain.Models.EntityModels
{
    using System;
    using System.Collections.Generic;
    using Fsel.Master.Domain.Entities;

    public class PlacementTestDetailModel
    {
        public Guid LevelId { get; set; }
        public string? LevelName { get; set; }
        public Guid ProgramId { get; set; }
        public string? ProgramName { get; set; }
        public Guid SubjectId { get; set; }
        public string? SubjectName { get; set; }
        public IList<PlacementTestModuleModel>? Modules { get; set; }
    }

    public class PlacementTestModuleModel
    {
        public IList<SkillScore>? SkillScores { get; set; }
    }
}
