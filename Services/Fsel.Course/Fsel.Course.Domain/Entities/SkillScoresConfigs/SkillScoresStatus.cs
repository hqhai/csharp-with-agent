// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Entities.SkillScoresConfigs
{
    using System.Text.Json.Serialization;
    using Fsel.Course.Domain.Enums;
    using Fsel.Shared.Enums;

    public class SkillScoresStatus : SkillScores
    {
        public EnumResultStatus? Status { get; set; }
    }
}
