// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Infrastructure.Maps
{
    using AutoMapper;
    using Fsel.Core.Extensions;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Identity.Domain.Models.CommandModels.UserSettings;
    using Fsel.Identity.Domain.Models.EntityModels;

    public class UserSettingProfile : Profile
    {
        public UserSettingProfile()
        {
            CreateMap<UserSetting, UserSettingModel>().IgnoreAllNonExisting();
            CreateMap<SaveUserSettingCommandModel, UserSetting>().ForMember(dest => dest.UserSenderSettings, opt => opt.Ignore()).IgnoreAllNonExisting();
            CreateMap<SaveUserSenderSetting, UserSenderSetting>().IgnoreAllNonExisting();
            CreateMap<UserSenderSetting, UserSenderSettingModel>().IgnoreAllNonExisting();
        }
    }
}
