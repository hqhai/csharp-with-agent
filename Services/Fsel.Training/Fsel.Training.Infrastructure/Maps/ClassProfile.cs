// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Training.Infrastructure.Maps
{
    using AutoMapper;
    using Fsel.Core.Extensions;
    using Fsel.Training.Domain.Entities;
    using Fsel.Training.Domain.Models.CommandModels.Classes;
    using Fsel.Training.Domain.Models.EntityModels;

    public class ClassProfile : Profile
    {
        public ClassProfile()
        {
            CreateMap<Class, ClassModel>().IgnoreAllNonExisting();
            CreateMap<Class, CourseClassModel>().IgnoreAllNonExisting();
            CreateMap<CreateClassCommandModel, Class>().IgnoreAllNonExisting();
            CreateMap<ClassLiveWorkFlow, ClassLiveWorkFlowModel>().IgnoreAllNonExisting();
        }
    }
}
