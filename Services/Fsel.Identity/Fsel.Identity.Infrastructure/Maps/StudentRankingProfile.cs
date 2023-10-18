// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Infrastructure.Maps
{
    using AutoMapper;
    using Fsel.Core.Extensions;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Identity.Domain.Models.CommandModels.StudentRanking;
    using Fsel.Identity.Domain.Models.EntityModels;
    using Fsel.Shared.Models.ShareModels;

    public class StudentRankingProfile : Profile
    {
        public StudentRankingProfile()
        {
            CreateMap<StudentRanking, StudentRankingModel>().IgnoreAllNonExisting();
            CreateMap<StudentRanking, StudentRankingRealTime>().IgnoreAllNonExisting();
            CreateMap<CreateStudentRankingCommandModel, StudentRanking>().IgnoreAllNonExisting();
        }
    }
}
