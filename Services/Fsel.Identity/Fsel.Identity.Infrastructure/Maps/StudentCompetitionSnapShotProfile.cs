// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Infrastructure.Maps
{
    using AutoMapper;
    using Fsel.Core.Extensions;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Identity.Domain.Models.CommandModels.StudentCompetitionSnapShot;
    using Fsel.Identity.Domain.Models.EntityModels;

    public class StudentCompetitionSnapShotProfile : Profile
    {
        public StudentCompetitionSnapShotProfile()
        {
            CreateMap<CreateStudentCompetitionSnapShotModel, StudentCompetitionSnapShot>().IgnoreAllNonExisting();
            CreateMap<StudentCompetitionSnapShotModel, CreateStudentCompetitionSnapShotModel>().IgnoreAllNonExisting();
        }
    }
}
