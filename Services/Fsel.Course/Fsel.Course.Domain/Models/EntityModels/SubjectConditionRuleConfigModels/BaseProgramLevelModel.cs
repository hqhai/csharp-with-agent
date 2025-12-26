// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels.SubjectConditionRuleConfigModels
{
    public class BaseProgramLevelModel
    {
        public IList<BaseProgramLevelDetailModel>? Levels { get; set; }
    }

    public class BaseProgramLevelDetailModel
    {
        public string? Name { get; set; }

        public Guid LevelId { get; set; }
    }
}
