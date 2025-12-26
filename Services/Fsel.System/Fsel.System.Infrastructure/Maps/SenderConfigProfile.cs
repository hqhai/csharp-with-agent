// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Infrastructure.Maps
{
    using AutoMapper;
    using Fsel.Core.Extensions;
    using Fsel.System.Domain.Entities;
    using Fsel.System.Domain.Models.EntityModels;

    public class SenderConfigProfile : Profile
    {
        public SenderConfigProfile()
        {
            CreateMap<SenderConfig, SenderConfigModel>().IgnoreAllNonExisting();
        }
    }
}
