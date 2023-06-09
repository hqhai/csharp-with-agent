// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Training.Infrastructure.Maps
{
    using AutoMapper;
    using Fsel.Training.Domain.Models.EntityModels;
    using Fsel.Core.Extensions;
    using Fsel.Training.Domain.Entities;

    public class ClassProfile : Profile
    {
        public ClassProfile()
        {
            CreateMap<Class, ClassModel>().IgnoreAllNonExisting();
            CreateMap<Class, CourseClassModel>().IgnoreAllNonExisting();
        }
    }
}
