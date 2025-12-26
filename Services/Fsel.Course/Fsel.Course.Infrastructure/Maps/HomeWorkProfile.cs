// Copyright (c) Atlantic. All rights reserved.

using AutoMapper;
using Fsel.Core.Extensions;
using Fsel.Course.Domain.Entities;
using Fsel.Course.Domain.Models.CommandModels.HomeWorks;
using Fsel.Course.Domain.Models.EntityModels;

namespace Fsel.Course.Infrastructure.Maps
{
    public class HomeWorkProfile : Profile
    {
        public HomeWorkProfile()
        {
            CreateMap<HomeWork, HomeWorkModel>().ForMember(p => p.SkillName, x => x.MapFrom(n => n.Skill != null ? n.Skill.Name : null));
            CreateMap<HomeWork, LessonHomeWorkResultModel>().IgnoreAllNonExisting();
            CreateMap<CreateHomeWorkCommandModel, HomeWork>().IgnoreAllNonExisting();
            CreateMap<UpdateHomeWorkCommandModel, HomeWork>().IgnoreAllNonExisting();
            CreateMap<HomeWork, HomeWorkExtraDtoModel>().IgnoreAllNonExisting();
        }
    }
}