// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Maps
{
    using AutoMapper;
    using Fsel.Core.Extensions;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Domain.Models.EntityModels.V1i1;

    public class LessonResultProfile : Profile
    {
        public LessonResultProfile()
        {
            CreateMap<LessonResult, LessonResultModel>().IgnoreAllNonExisting();
            CreateMap<LessonResult, LessonMockTestResultModel>()
            .ForMember(x => x.Type, p => p.MapFrom(o => nameof(Lesson)));
        }
    }
}
