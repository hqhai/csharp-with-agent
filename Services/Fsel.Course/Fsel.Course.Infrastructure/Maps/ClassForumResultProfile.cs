// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Maps
{
    using AutoMapper;
    using Fsel.Core.Extensions;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Models.CommandModels.ClassForumResults;
    using Fsel.Course.Domain.Models.EntityModels;

    public class ClassForumResultProfile : Profile
    {
        public ClassForumResultProfile()
        {
            CreateMap<ClassForumResult, ClassForumResultModel>().IgnoreAllNonExisting();
            CreateMap<CreateClassForumResultCommandModel, ClassForumResult>().IgnoreAllNonExisting();
            CreateMap<RateClassForumResultCommandModel, ClassForumResult>().IgnoreAllNonExisting();
            CreateMap<ClassForumResult, ClassForumResultInfoModel>()
                .ForMember(p => p.UnitId, x => x.MapFrom(n => n.LessonResult!.UnitId))
                .ForMember(p => p.CourseId, x => x.MapFrom(n => n.LessonResult!.CourseId));
        }
    }
}
