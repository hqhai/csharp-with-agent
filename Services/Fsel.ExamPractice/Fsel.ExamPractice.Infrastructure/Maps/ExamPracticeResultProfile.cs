// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.ExamPractice.Infrastructure.Maps
{
    using AutoMapper;
    using Fsel.Core.Extensions;
    using Fsel.ExamPractice.Domain.Entities;
    using Fsel.ExamPractice.Domain.Enums;
    using Fsel.ExamPractice.Domain.Models.EntityModels.Bases;
    using Fsel.ExamPractice.Domain.Models.EntityModels.ExamPractices;
    using Fsel.Shared.Helpers;

    public class ExamPracticeResultProfile : Profile
    {
        public ExamPracticeResultProfile()
        {
            CreateMap<ExamPracticeResult, ExamPracticeResultModel>()
                .ForMember(x => x.Score, p => p.MapFrom(o => o.SkillScores != null && o.SkillScores.Any() ? NumberHelper.RoundNumberDouble(o.SkillScores.Average(x => x.Scores)) : default))
                .ForMember(p => p.RemainingTime, x => x.MapFrom(n =>
                n.PracticeMode == EnumPracticeMode.Practice ? n.Config != null && n.Config.ExecutionTime != null && n.Config.ExecutionTime - n.WorkingTime > 0 ? n.Config.ExecutionTime - n.WorkingTime : default :
                n.ExamPractice != null && n.ExamPractice.ExecutionTime - n.WorkingTime > 0 ? n.ExamPractice.ExecutionTime - n.WorkingTime : default)
            );
            CreateMap<ExamPracticeSectionResult, ExamPracticeSectionResultModel>()
                .ForMember(p => p.RemainingTime, x => x.MapFrom(n => n.ExamPracticeSection != null && n.ExamPracticeSection.Config != null && n.ExamPracticeSection.Config.ExecutionTime != null && n.ExamPracticeSection.Config.ExecutionTime - n.WorkingTime > 0 ? n.ExamPracticeSection.Config.ExecutionTime - n.WorkingTime : default));
            CreateMap<ExamPracticeResult, ExamPracticeResultReportModel>()
            .ForMember(x => x.Score, p => p.MapFrom(o => o.SkillScores != null && o.SkillScores.Any() ? NumberHelper.RoundNumberDouble(o.SkillScores.Average(x => x.Scores)) : default))
            .ForMember(p => p.RemainingTime, x => x.MapFrom(n =>
                n.PracticeMode == EnumPracticeMode.Practice ? n.Config != null && n.Config.ExecutionTime != null && n.Config.ExecutionTime - n.WorkingTime > 0 ? n.Config.ExecutionTime - n.WorkingTime : default :
                n.ExamPractice != null && n.ExamPractice.ExecutionTime - n.WorkingTime > 0 ? n.ExamPractice.ExecutionTime - n.WorkingTime : default)
            );
            CreateMap<ExamPracticeAnswer, AnswerModel>().IgnoreAllNonExisting();
        }
    }
}
