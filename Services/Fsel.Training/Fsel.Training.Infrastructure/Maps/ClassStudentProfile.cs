// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Training.Infrastructure.Maps
{
    using AutoMapper;
    using Fsel.Core.Extensions;
    using Fsel.Training.Domain.Entities;
    using Fsel.Training.Domain.Models.EntityModels;

    public class ClassStudentProfile : Profile
    {
        public ClassStudentProfile()
        {
            CreateMap<ClassStudent, ClassStudentModel>().IgnoreAllNonExisting();
        }
    }
}
