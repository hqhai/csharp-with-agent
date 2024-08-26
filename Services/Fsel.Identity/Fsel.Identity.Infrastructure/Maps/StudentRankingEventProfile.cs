// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Infrastructure.Maps
{
    using AutoMapper;
    using Fsel.Core.Extensions;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Identity.Domain.Models.CommandModels.StudentRanking;
    using Fsel.Identity.Domain.Models.EntityModels;

    public class StudentRankingEventProfile : Profile
    {
        public StudentRankingEventProfile()
        {
            CreateMap<StudentRankingEvent, StudentRankingEventModel>().IgnoreAllNonExisting();
            CreateMap<StudentRankingEventModel, StudentRankingEvent>().IgnoreAllNonExisting();
            CreateMap<StudentRankingEventCommandModel, StudentRankingEvent>().IgnoreAllNonExisting();
            CreateMap<StudentRankingModel, StudentRankingEvent>().IgnoreAllNonExisting();
        }
    }
}
