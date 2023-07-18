// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Infrastructure.Maps
{
    using AutoMapper;
    using Fsel.Core.Extensions;
    using Fsel.System.Domain.Entities;
    using Fsel.System.Domain.Models.CommandModels.TeachingCosts;
    using Fsel.System.Domain.Models.EntityModels;

    public class TeachingCostProfile : Profile
    {
        public TeachingCostProfile()
        {
            CreateMap<TeachingCost, TeachingCostModel>().IgnoreAllNonExisting();
            CreateMap<SaveTeachingCostCommandModel, TeachingCost>().IgnoreAllNonExisting();
        }
    }
}
