// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Infrastructure.Maps
{
    using AutoMapper;
    using Fsel.Core.Extensions;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Identity.Domain.Models.EntityModels;

    public class StudentCompetitionEventProfile : Profile
    {
        public StudentCompetitionEventProfile()
        {
            CreateMap<CompetitionEvent, CompetitionEventsModel>().IgnoreAllNonExisting();
            CreateMap<StudentCompetitionEvent, StudentCompetitionEventsModel>().IgnoreAllNonExisting();
        }
    }
}
