// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Maps
{
    using AutoMapper;
    using Fsel.Core.Extensions;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Models.CommandModels.SectionTimeCodes;
    using Fsel.Course.Domain.Models.CommandModels.Skills;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Domain.Models.EntityModels.SkillModels;

    public class SkillProfile : Profile
    {
        public SkillProfile()
        {
            CreateMap<Skill, SkillModel>().IgnoreAllNonExisting();
            CreateMap<Skill, SkillViewModel>().IgnoreAllNonExisting();
            CreateMap<CreateSkillCommandModel, Skill>().IgnoreAllNonExisting();
            CreateMap<UpdateSkillCommandModel, Skill>().IgnoreAllNonExisting();
        }
    }
}
