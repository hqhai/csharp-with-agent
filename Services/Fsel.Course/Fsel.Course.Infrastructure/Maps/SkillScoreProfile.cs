// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Maps
{
    using AutoMapper;
    using Fsel.Core.Extensions;
    using Fsel.Course.Domain.Entities.SkillScoresConfigs;

    public class SkillScoreProfile : Profile
    {
        public SkillScoreProfile()
        {
            CreateMap<SkillScores, TestSkillScores>().IgnoreAllNonExisting();
        }
    }
}
