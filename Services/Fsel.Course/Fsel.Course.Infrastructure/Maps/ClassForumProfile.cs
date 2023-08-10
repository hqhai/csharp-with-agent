// Copyright (c) Atlantic. All rights reserved.

using AutoMapper;
using Fsel.Core.Extensions;
using Fsel.Course.Domain.Entities;
using Fsel.Course.Domain.Models.CommandModels.ClassForums;
using Fsel.Course.Domain.Models.EntityModels;

namespace Fsel.Course.Infrastructure.Maps
{
    public class ClassForumProfile : Profile
    {
        public ClassForumProfile()
        {
            CreateMap<ClassForum, ClassForumModel>().IgnoreAllNonExisting();
            CreateMap<CreateClassForumCommandModel, ClassForum>().IgnoreAllNonExisting();    
            CreateMap<ClassForum, ClassForumByStudentModel>().IgnoreAllNonExisting();
        }
    }
}
