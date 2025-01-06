// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Infrastructure.Maps
{
    using AutoMapper;
    using Fsel.Core.Extensions;
    using Fsel.System.Domain.Entities;
    using Fsel.System.Domain.Models.CommandModels.FselRatings;
    using Fsel.System.Domain.Models.EntityModels;

    public class FselRatingProfile : Profile
    {
        public FselRatingProfile()
        {
            CreateMap<FselRating, FselRatingModel>().IgnoreAllNonExisting();
            CreateMap<CreateFselRatingCommandModel, FselRating>().IgnoreAllNonExisting();
        }
    }
}
