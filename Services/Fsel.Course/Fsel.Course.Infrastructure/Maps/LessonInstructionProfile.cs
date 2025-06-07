// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Maps
{
    using AutoMapper;
    using Fsel.Core.Extensions;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Models.CommandModels.LessonInstructions;
    using Fsel.Course.Domain.Models.EntityModels;

    public class LessonInstructionProfile : Profile
    {
        public LessonInstructionProfile()
        {
            CreateMap<LessonInstruction, LessonInstructionModel>().ForMember(p => p.SkillName, x => x.MapFrom(n => n.Skill != null ? n.Skill.Name : null));
            CreateMap<CreateLessonInstructionCommandModel, LessonInstruction>().IgnoreAllNonExisting();
            CreateMap<UpdateLessonInstructionCommandModel, LessonInstruction>().IgnoreAllNonExisting();
        }
    }
}
