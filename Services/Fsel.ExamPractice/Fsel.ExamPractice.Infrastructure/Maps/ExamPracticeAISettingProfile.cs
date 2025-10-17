// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.ExamPractice.Infrastructure.Maps
{
    using AutoMapper;
    using Fsel.Core.Extensions;
    using Fsel.ExamPractice.Domain.Entities;
    using Fsel.ExamPractice.Domain.Models.CommandModels.ExamPracticeAISettings;
    using Fsel.ExamPractice.Domain.Models.EntityModels;
    using Fsel.ExamPractice.Domain.Models.EntityModels.ExamPracticeAnswers;
    using Fsel.Shared.Models.ShareModels;

    public class ExamPracticeAISettingProfile : Profile
    {
        public ExamPracticeAISettingProfile()
        {
            CreateMap<ExamPracticeAISetting, ExamPracticeAISettingModel>().IgnoreAllNonExisting();
            CreateMap<ExamPracticeAICriteriaSetting, ExamPracticeAICriteriaSettingModel>().IgnoreAllNonExisting();
            CreateMap<ExamPracticeAICriteriaSettingModel, ExamPracticeAICriteriaSetting>().IgnoreAllNonExisting();
            CreateMap<ExamPracticeAISettingModel, ExamPracticeAISetting>().IgnoreAllNonExisting();
            CreateMap<ExamPracticeAISettingCommandModel, ExamPracticeAISetting>().ForMember(m => m.ExamPracticeAICriteriaSettings, opt => opt.Ignore()).IgnoreAllNonExisting();
            CreateMap<ExamPracticeAICriteriaSettingCommandModel, ExamPracticeAICriteriaSetting>().IgnoreAllNonExisting();

            CreateMap<ExamPracticeAIGradingDataModel, ExamPracticeAIGradingModel>().ForMember(x => x.BandScore, x => x.MapFrom(y => y.BandScore));
        }
    }
}
