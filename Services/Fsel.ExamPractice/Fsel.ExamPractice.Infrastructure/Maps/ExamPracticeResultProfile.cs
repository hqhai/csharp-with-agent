// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.ExamPractice.Infrastructure.Maps
{
    using AutoMapper;
    using Fsel.Core.Extensions;
    using Fsel.ExamPractice.Domain.Entities;
    using Fsel.ExamPractice.Domain.Models.EntityModels.ExamPractices;
    using Fsel.Shared.Helpers;

    public class ExamPracticeResultProfile : Profile
    {
        public ExamPracticeResultProfile()
        {
            CreateMap<ExamPracticeResult, ExamPracticeResultModel>().IgnoreAllNonExisting();
            CreateMap<ExamPracticeSectionResult, ExamPracticeSectionResultModel>().IgnoreAllNonExisting();
            CreateMap<ExamPracticeResult, ExamPracticeResultReportModel>()
            .ForMember(x => x.Score, p => p.MapFrom(o => o.SkillScores != null && o.SkillScores.Any() ? NumberHelper.RoundNumberDouble(o.SkillScores.Average(x => x.Scores)) : default));
        }
    }
}
