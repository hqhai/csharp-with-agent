// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Infrastructure.Maps
{
    using AutoMapper;
    using Fsel.Core.Extensions;
    using Fsel.System.Domain.Entities;
    using Fsel.System.Domain.Models.CommandModels.BannerSettings;
    using Fsel.System.Domain.Models.EntityModels;

    public class BannerSettingProfile : Profile
    {
        public BannerSettingProfile()
        {
            CreateMap<BannerSetting, BannerSettingModel>().IgnoreAllNonExisting();
            CreateMap<CreateBannerSettingCommandModel, BannerSetting>().IgnoreAllNonExisting();
        }
    }
}
