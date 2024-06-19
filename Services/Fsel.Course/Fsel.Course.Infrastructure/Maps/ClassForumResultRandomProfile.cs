// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Maps
{
    using AutoMapper;
    using Fsel.Core.Extensions;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Models.EntityModels;

    public class ClassForumResultRandomProfile : Profile
    {
        public ClassForumResultRandomProfile()
        {
            CreateMap<ClassForumResult, ClassForumResultRandom>().ForMember(dest => dest.ClassForumResultId, opt => opt.MapFrom(src => src.Id));
        }
    }
}
