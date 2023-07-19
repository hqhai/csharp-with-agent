// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Infrastructure.Maps
{
    using AutoMapper;
    using Fsel.Core.Extensions;
    using Fsel.System.Domain.Entities;
    using Fsel.System.Domain.Models.CommandModels.CourseTimeConfigs;
    using Fsel.System.Domain.Models.EntityModels;

    public class CourseTimeConfigProfile : Profile
    {
        public CourseTimeConfigProfile()
        {
            CreateMap<CourseTimeConfig, CourseTimeConfigModel>().IgnoreAllNonExisting();
            CreateMap<SetEnrollmentWeekToClassCommandModel, CourseTimeConfig>().IgnoreAllNonExisting();
            CreateMap<SetMonthToClassCommandModel, CourseTimeConfig>().IgnoreAllNonExisting();
            CreateMap<CourseTimeConfigModel, CourseTimeConfig>().IgnoreAllNonExisting();
        }
    }
}
