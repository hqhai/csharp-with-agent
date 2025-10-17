// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Maps
{
    using AutoMapper;
    using Fsel.Core.Extensions;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Models.CommandModels.HomeWorkConfigs;
    using Fsel.Course.Domain.Models.EntityModels;

    public class HomeWorkConfigProfile : Profile
    {
        public HomeWorkConfigProfile()
        {
            CreateMap<SaveHomeWorkConfigCommandModel, HomeWorkConfig>().IgnoreAllNonExisting();
            CreateMap<HomeWorkConfig, HomeWorkConfigModel>().IgnoreAllNonExisting();
        }
    }
}
