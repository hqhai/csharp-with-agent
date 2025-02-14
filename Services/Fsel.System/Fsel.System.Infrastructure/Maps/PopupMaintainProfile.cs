// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Infrastructure.Maps
{
    using AutoMapper;
    using Fsel.Core.Extensions;
    using Fsel.System.Domain.Entities;
    using Fsel.System.Domain.Models.CommandModels.MaintainConfigs;
    using Fsel.System.Domain.Models.EntityModels;
    using global::System;
    using global::System.Collections.Generic;
    using global::System.Linq;
    using global::System.Text;
    using global::System.Threading.Tasks;

    public class PopupMaintainProfile : Profile
    {
        public PopupMaintainProfile()
        {
            CreateMap<PopupMaintainModel, Banner>().IgnoreAllNonExisting();
            CreateMap<CreatePopupMaintainCommandModel, Banner>().IgnoreAllNonExisting();
            CreateMap<Banner, PopupMaintainModel>().IgnoreAllNonExisting();
        }
    }
}
