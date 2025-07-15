// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Maps
{
    using AutoMapper;
    using Fsel.Core.Extensions;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Domain.Models.EntityModels.V1i1;
    using Fsel.Shared.Helpers;

    public class MockTestResultProfile : Profile
    {
        public MockTestResultProfile()
        {
            CreateMap<MockTestResult, MockTestResultModel>()
                .ForMember(m => m.MockTestScores, opt => opt.Ignore())
                .ForMember(x => x.Scores, p => p.MapFrom(o => o.SkillScores != null && o.SkillScores.Any() ? NumberHelper.RoundNumberDouble(o.SkillScores.Average(x => x.Scores)) : default))
                .ForMember(x => x.ProgressPercent, p => p.MapFrom(x => x.MockTest != null ? NumberHelper.GetPercent(x.SectionGroupResults.Where(y => y.Status == EnumResultStatus.Done).Count(), x.MockTest.MockTestSections.Count, 0) : default));

            CreateMap<MockTestResult, TestResultRankingModel>()
                .ForMember(x => x.Score, p => p.MapFrom(o => o.SkillScores != null && o.SkillScores.Any() ? NumberHelper.RoundNumberDouble(o.SkillScores.Average(x => x.Scores)) : default));

            CreateMap<MockTestResult, MockTestResultReportModel>()
              .ForMember(x => x.Score, p => p.MapFrom(o => o.SkillScores != null && o.SkillScores.Any() ? NumberHelper.RoundNumberDouble(o.SkillScores.Average(x => x.Scores)) : default));

            CreateMap<MockTestResult, LessonMockTestResultModel>()
            .ForMember(x => x.Scores, p => p.MapFrom(o => o.SkillScores != null && o.SkillScores.Any() ? NumberHelper.RoundNumberDouble(o.SkillScores.Average(x => x.Scores)) : default))
            .ForMember(x => x.Type, p => p.MapFrom(o => nameof(EnumMockTestType.SkillMockTest)))
            .ForMember(x => x.ObjectId, p => p.MapFrom(o => o.MockTestId));

            CreateMap<MockTestResult, CourseUnitMockTestResultModel>().IgnoreAllNonExisting();
        }
    }
}
