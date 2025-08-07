// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Maps
{
    using AutoMapper;
    using Fsel.Core.Extensions;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Shared.Helpers;

    public class FinalTestResultProfile : Profile
    {
        public FinalTestResultProfile()
        {
            CreateMap<FinalTestResult, FinalTestResultModel>()
            .ForMember(x => x.ProgressPercent, p => p.MapFrom(x => x.FinalTest != null ? NumberHelper.GetPercent(x.SectionGroupResults.Where(y => y.Status == EnumResultStatus.Done).Count(), x.FinalTest.FinalTestSections.Count, 0) : default));
            CreateMap<FinalTestResult, TestResultRankingModel>()
            .ForMember(x => x.Score, p => p.MapFrom(o => o.CorrectCount));
            CreateMap<FinalTestResult, TestResultReportModel>()
            .ForMember(x => x.Score, p => p.MapFrom(o => o.CorrectCount));
            CreateMap<FinalTestResult, CourseUnitMockTestResultModel>().IgnoreAllNonExisting();
        }
    }
}
