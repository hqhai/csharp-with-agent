// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Infrastructure.Maps
{
    using AutoMapper;
    using Fsel.Core.Extensions;
    using Fsel.System.Domain.Entities;
    using Fsel.System.Domain.Models.CommandModels.CourseSuggestConfigs;
    using Fsel.System.Domain.Models.EntityModels;

    public class CourseSuggestConfigProfile : Profile
    {
        public CourseSuggestConfigProfile()
        {
            CreateMap<CourseSuggestConfig, CourseSuggestConfigModel>().IgnoreAllNonExisting();
            CreateMap<CreateCourseSuggestConfigCommandModel, CourseSuggestConfig>().IgnoreAllNonExisting();
            CreateMap<UpdateCourseSuggestConfigCommandModel, CourseSuggestConfig>().IgnoreAllNonExisting();
            CreateMap<CourseSuggestConfig, CourseSuggestConfigStudentModel>().IgnoreAllNonExisting();
        }
    }
}
