// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Infrastructure.Maps
{
    using AutoMapper;
    using Fsel.Core.Extensions;
    using Fsel.System.Domain.Entities;
    using Fsel.System.Domain.Models.CommandModels.ReferralDiscountConfigs;
    using Fsel.System.Domain.Models.EntityModels;

    public class ReferralDiscountConfigProfile : Profile
    {
        public ReferralDiscountConfigProfile()
        {
            CreateMap<ReferralDiscountConfig, ReferralDiscountConfigModel>().IgnoreAllNonExisting();
            CreateMap<SaveReferralDiscountConfigCommandModel, ReferralDiscountConfig>().IgnoreAllNonExisting();
        }
    }
}
