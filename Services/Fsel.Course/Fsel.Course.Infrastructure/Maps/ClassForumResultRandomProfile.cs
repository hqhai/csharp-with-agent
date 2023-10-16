// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Maps
{
    using AutoMapper;
    using Fsel.Course.Domain.Entities;

    public class ClassForumResultRandomProfile : Profile
    {
        public ClassForumResultRandomProfile()
        {
            CreateMap<ClassForumResult, ClassForumResultRandom>().ForMember(dest => dest.ClassForumId, opt => opt.MapFrom(src => src.Id));
        }
    }
}
