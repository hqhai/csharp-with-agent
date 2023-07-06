// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Entities.SkillScoresConfigs
{
    using System.Collections.Generic;
    using System.Text.Json.Serialization;
    using Fsel.Course.Domain.Enums;

    public class VideoSkillScores
    {
        [JsonRequired]
        public EnumTimeCodeType Type { get; set; }
        [JsonRequired]
        public IList<SkillScores>? SkillScores { get; set; }
    }
}
