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
        private readonly int _maxTotalScore = 100;
        public StudentRankingProfile()
        {
            CreateMap<StudentRanking, StudentRankingModel>().ForMember(x => x.TotalScore, c => c.MapFrom(p => p.TotalScore > _maxTotalScore ? _maxTotalScore : p.TotalScore));
            CreateMap<StudentRanking, StudentRankingRealTime>().IgnoreAllNonExisting();
            CreateMap<CreateStudentRankingCommandModel, StudentRanking>().IgnoreAllNonExisting();
            CreateMap<RegisterStudentForEventCommandModel, EventRegistration>().IgnoreAllNonExisting();
        }
    }
}
