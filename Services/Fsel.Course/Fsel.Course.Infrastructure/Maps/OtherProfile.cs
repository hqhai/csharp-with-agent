// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Maps
{
    using AutoMapper;
    using Fsel.Core.Extensions;
    using Fsel.Course.Domain.Entities.BandScoresConfigs;

    public class OtherProfile : Profile
    {
        public OtherProfile()
        {
            CreateMap<BandScores, BandScoresReport>().ForMember(m => m.Scores, p => p.MapFrom(o => o.Scores - 0.5)).IgnoreAllNonExisting();
        }
    }
}
