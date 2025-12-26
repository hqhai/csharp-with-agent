// Copyright (c) Atlantic. All rights reserved.

using AutoMapper;
using Fsel.Core.Extensions;
using Fsel.Course.Domain.Entities;
using Fsel.Course.Domain.Models.CommandModels.Exercises;
using Fsel.Course.Domain.Models.EntityModels;

namespace Fsel.Course.Infrastructure.Maps
{
    public class ExerciseProfile : Profile
    {
        public ExerciseProfile()
        {
            CreateMap<Exercise, ExerciseModel>().ForMember(p => p.SkillName, x => x.MapFrom(n => n.Skill != null ? n.Skill.Name : null));
            CreateMap<CreateExerciseCommandModel, Exercise>().IgnoreAllNonExisting();
            CreateMap<UpdateExerciseCommandModel, Exercise>().IgnoreAllNonExisting();
            CreateMap<UpdateExerciseCommandModel, CreateExerciseCommandModel>().ForMember(p => p.Id, x => x.Ignore()).IgnoreAllNonExisting();
        }
    }
}
