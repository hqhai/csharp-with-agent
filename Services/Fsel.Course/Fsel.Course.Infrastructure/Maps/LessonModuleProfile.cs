// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Maps
{
    using AutoMapper;
    using Fsel.Core.Extensions;
    using Fsel.Course.Domain.Entities.V1i1;
    using Fsel.Course.Domain.Models.CommandModels.LessonModules;
    using Fsel.Course.Domain.Models.EntityModels.V1i1;

    public class LessonModuleProfile : Profile
    {
        public LessonModuleProfile()
        {
            CreateMap<LessonModule, LessonModuleModel>().IgnoreAllNonExisting();
            CreateMap<CreateLessonModuleCommandModel, LessonModule>()
                    .ForMember(p => p.ClassForum, x => x.Ignore())
                    .ForMember(p => p.HomeWork, x => x.Ignore());
        }
    }
}
