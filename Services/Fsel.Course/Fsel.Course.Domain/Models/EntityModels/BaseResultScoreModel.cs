// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    using Fsel.Course.Domain.Entities.SkillScoresConfigs;

    public class BaseResultScoreModel : BaseResultModel
    {
        public IList<SkillScores>? SkillScores { get; set; }
    }
}
