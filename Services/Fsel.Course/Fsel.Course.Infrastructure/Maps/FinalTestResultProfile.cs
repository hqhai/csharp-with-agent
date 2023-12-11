// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Maps
{
    using AutoMapper;
    using Fsel.Core.Extensions;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Models.EntityModels;

    public class FinalTestResultProfile : Profile
    {
        public FinalTestResultProfile()
        {
            CreateMap<FinalTestResult, FinalTestResultModel>().IgnoreAllNonExisting();
            CreateMap<FinalTestResult, TestResultRankingModel>()
            .ForMember(x => x.Score, p => p.MapFrom(o => o.CorrectCount));
            CreateMap<FinalTestResult, TestResultReportModel>()
            .ForMember(x => x.Score, p => p.MapFrom(o => o.CorrectCount));
        }
    }
}
