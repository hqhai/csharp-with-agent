// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Maps
{
    using AutoMapper;
    using Fsel.Core.Extensions;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Domain.Models.EntityModels.V1i1;

    public class MockTestResultProfile : Profile
    {
        public MockTestResultProfile()
        {
            CreateMap<MockTestResult, MockTestResultModel>().IgnoreAllNonExisting();
            CreateMap<MockTestResult, TestResultRankingModel>()
                .ForMember(x => x.Score, p => p.MapFrom(o => o.SkillScores != null && o.SkillScores.Any() ? o.SkillScores.Average(x => x.Scores) : default));
            CreateMap<MockTestResult, TestResultReportModel>()
                .ForMember(x => x.Score, p => p.MapFrom(o => o.SkillScores != null && o.SkillScores.Any() ? o.SkillScores.Average(x => x.Scores) : default));
            CreateMap<MockTestResult, LessonMockTestResultModel>()
            .ForMember(x => x.Type, p => p.MapFrom(o => nameof(EnumMockTestType.SkillMockTest)))
            .ForMember(x => x.ObjectId, p => p.MapFrom(o => o.MockTestId));
        }
    }
}
