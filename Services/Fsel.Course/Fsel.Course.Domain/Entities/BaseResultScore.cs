// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Entities
{
    using System.ComponentModel.DataAnnotations.Schema;
    using Fsel.Common.Helpers;
    using Fsel.Course.Domain.Entities.SkillScoresConfigs;

    public class BaseResultScore : BaseResult
    {
        public string? SkillScoresStr { get; set; }

        [NotMapped]
        public IList<SkillScores>? SkillScores
        {
            get
            {
                return ConvertHelper.Deserialize<IList<SkillScores>>(SkillScoresStr);
            }
            set { SkillScoresStr = ConvertHelper.Serialize(value); }
        }
    }
}
