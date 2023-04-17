// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Training.Infrastructure.Maps
{
    using AutoMapper;
    using Fsel.Training.Doman.Models.EntityModels;
    using Fsel.Core.Extensions;
    using Fsel.Training.Doman.Entities;

    public class ClassProfile : Profile
    {
        public ClassProfile()
        {
            CreateMap<Class, ClassModel>().IgnoreAllNonExisting();
            CreateMap<Class, CourseClassModel>().IgnoreAllNonExisting();
        }
    }
}
