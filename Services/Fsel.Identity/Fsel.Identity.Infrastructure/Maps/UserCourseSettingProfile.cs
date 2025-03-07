// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Infrastructure.Maps
{
    using AutoMapper;
    using Fsel.Core.Extensions;
    using Fsel.Identity.Domain.Entities;

    public class UserCourseSettingProfile : Profile
    {
        public UserCourseSettingProfile()
        {
            CreateMap<UserCourseSetting, Domain.Models.EntityModels.UserCourseSettingModel>().IgnoreAllNonExisting();
            CreateMap<UserCourseSetting, Shared.Models.ShareModels.EntityModels.UserCourseSettingModel>().IgnoreAllNonExisting();
        }
    }
}
