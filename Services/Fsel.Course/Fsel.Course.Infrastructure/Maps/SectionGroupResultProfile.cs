// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Maps
{
    using AutoMapper;
    using Fsel.Core.Extensions;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Entities.BandScoresConfigs;
    using Fsel.Course.Domain.Models.EntityModels;

    public class SectionGroupResultProfile : Profile
    {
        public SectionGroupResultProfile()
        {
            CreateMap<SectionGroupResult, SectionGroupResultModel>().IgnoreAllNonExisting();
            CreateMap<SectionGroupResult, SectionGroupResultReportModel>().IgnoreAllNonExisting();
            CreateMap<BandScores, BandScoresReport>().IgnoreAllNonExisting();
        }
    }
}
