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
            CreateMap<ClassForum, ClassForumModel>().ForMember(p => p.SkillName, x => x.MapFrom(n => n.Skill != null ? n.Skill.Name : null));
            CreateMap<CreateClassForumCommandModel, ClassForum>().IgnoreAllNonExisting();
            CreateMap<ClassForum, ClassForumByStudentModel>().IgnoreAllNonExisting();

            CreateMap<ClassForum, Fsel.Course.Domain.Models.EntityModels.V1i1.ClassForumModel>().ForMember(p => p.NameSkill, x => x.MapFrom(n => n.Skill != null ? n.Skill.Name : null));
            CreateMap<Fsel.Course.Domain.Models.CommandModels.ClassForums.V1i1.CreateClassForumCommandModel, ClassForum>().IgnoreAllNonExisting();
        }
    }
}
