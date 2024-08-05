// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Maps
{
    using AutoMapper;
    using Fsel.Core.Extensions;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Models.CommandModels.ProsodyCommandModel;
    using Fsel.Course.Domain.Models.EntityModels;

    public class ProsodyScoreProfile : Profile
    {
        public ProsodyScoreProfile()
        {
            CreateMap<ProsodyScore, ProsodyScoreModel>().IgnoreAllNonExisting();
            CreateMap<ProsodyCommandModel, ProsodyScore>().IgnoreAllNonExisting();
        }
    }
}
