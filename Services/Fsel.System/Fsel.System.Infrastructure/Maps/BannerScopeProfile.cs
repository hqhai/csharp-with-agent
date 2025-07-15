// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Infrastructure.Maps
{
    using AutoMapper;
    using Fsel.Core.Extensions;
    using Fsel.System.Domain.Entities;
    using Fsel.System.Domain.Models.CommandModels.BannerScopes;
    using Fsel.System.Domain.Models.EntityModels;

    public class BannerScopeProfile : Profile
    {
        public BannerScopeProfile()
        {
            CreateMap<BannerScope, BannerScopeModel>().IgnoreAllNonExisting();
            CreateMap<CreateBannerScopeCommandModel, BannerScope>().IgnoreAllNonExisting();
            CreateMap<BannerScope, CreateBannerScopeCommandModel>().IgnoreAllNonExisting();
        }
    }
}
