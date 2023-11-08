// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Infrastructure.Maps
{
    using AutoMapper;
    using Fsel.Core.Extensions;
    using Fsel.Interaction.Domain.Entities;
    using Fsel.Interaction.Domain.Models.CommandModels.Flags;
    using Fsel.Interaction.Domain.Models.EntityModels;

    public class FlagProfile : Profile
    {
        public FlagProfile()
        {
            CreateMap<Flag, FlagModel>().IgnoreAllNonExisting();
            CreateMap<RateFlagCommandModel, Flag>().IgnoreAllNonExisting();
            CreateMap<ApproveFlagCommandModel, Flag>().IgnoreAllNonExisting();
        }
    }
}
