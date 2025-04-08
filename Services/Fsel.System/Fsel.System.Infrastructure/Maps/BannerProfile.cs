// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Infrastructure.Maps
{
    using AutoMapper;
    using Fsel.Core.Extensions;
    using Fsel.Shared.Models.ShareModels;
    using Fsel.System.Domain.Entities;
    using Fsel.System.Domain.Models.CommandModels.Banners;
    using Fsel.System.Domain.Models.EntityModels;

    public class BannerProfile : Profile
    {
        public BannerProfile()
        {
            CreateMap<Banner, BannerModel>().IgnoreAllNonExisting();
            CreateMap<CreateBannerCommandModel, Banner>().IgnoreAllNonExisting();
            CreateMap<UpdateBannerCommandModel, Banner>().ForMember(x => x.BannerScopes, c => c.Ignore())
                                                         .ForMember(x => x.BannerImages, c => c.Ignore())
                                                         .IgnoreAllNonExisting();
            CreateMap<Banner, BannerStudentQueueModel>().ForMember(x => x.BannerId, x => x.MapFrom(y => y.Id));
        }
    }
}
