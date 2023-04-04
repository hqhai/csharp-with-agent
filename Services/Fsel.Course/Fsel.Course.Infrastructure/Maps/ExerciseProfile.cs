// Copyright (c) Atlantic. All rights reserved.

using AutoMapper;
using Fsel.Core.Extensions;
using Fsel.Course.Domain.Entities;
using Fsel.Course.Domain.Models.CommandModels.Exercises;

namespace Fsel.Course.Infrastructure.Maps
{
    public class ExerciseProfile : Profile
    {
        public ExerciseProfile()
        {
            //CreateMap<VideoTimeCode, VideoTimeCodeModel>().IgnoreAllNonExisting();
            CreateMap<CreateExerciseCommandModel, Exercise>().IgnoreAllNonExisting();
            CreateMap<UpdateExerciseCommandModel, Exercise>().IgnoreAllNonExisting();
        }
    }
}
