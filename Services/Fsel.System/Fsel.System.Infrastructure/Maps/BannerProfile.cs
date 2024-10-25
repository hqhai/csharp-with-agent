// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Infrastructure.Maps
{
    using AutoMapper;
    using Fsel.Core.Extensions;
    using Fsel.System.Domain.Entities;
    using Fsel.System.Domain.Models.CommandModels.Banners;
    using Fsel.System.Domain.Models.EntityModels;

    public class BannerProfile : Profile
    {
        public BannerProfile()
        {
            CreateMap<Banner, BannerModel>().IgnoreAllNonExisting();
            CreateMap<CreateBannerCommandModel, Banner>().IgnoreAllNonExisting();
            CreateMap<UpdateBannerCommandModel, Banner>().IgnoreAllNonExisting();
        }
    }
}
