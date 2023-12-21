// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Infrastructure.Maps
{
    using AutoMapper;
    using Fsel.Core.Extensions;
    using Fsel.System.Domain.Entities;
    using Fsel.System.Domain.Models.EntityModels;

    public class FocusTimeConfigProfile : Profile
    {
        public FocusTimeConfigProfile()
        {
            CreateMap<FocusTimeConfig, FocusTimeConfigModel>().IgnoreAllNonExisting();
        }
    }
}
