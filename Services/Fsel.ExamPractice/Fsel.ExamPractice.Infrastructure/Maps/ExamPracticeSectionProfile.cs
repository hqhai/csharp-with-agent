// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.ExamPractice.Infrastructure.Maps
{
    using AutoMapper;
    using Fsel.Core.Extensions;
    using Fsel.ExamPractice.Domain.Entities;
    using Fsel.ExamPractice.Domain.Models.CommandModels.ExamPracticeSections;
    using Fsel.ExamPractice.Domain.Models.EntityModels;
    using Fsel.ExamPractice.Domain.Models.EntityModels.ExamPractices;

    public class ExamPracticeSectionProfile : Profile
    {
        public ExamPracticeSectionProfile()
        {
            CreateMap<ExamPracticeSection, ExamPracticeSection>().IgnoreAllNonExisting();
            CreateMap<ExamPracticeSection, ExamPracticeSectionModel>().IgnoreAllNonExisting();
            CreateMap<ExamPracticeSection, ExamPracticeSectionDetailModel>().ForMember(x => x.QuestionIds, x => x.MapFrom(y => y.Questions.OrderBy(x => x.CreatedDate).Select(x => x.Id)));
            CreateMap<CreateExamPracticeSectionCommandModel, ExamPracticeSection>().ForMember(m => m.ExamPracticeSections, opt => opt.Ignore()).ForMember(m => m.Questions, opt => opt.Ignore()).ForMember(m => m.ExamPracticeAISettings, opt => opt.Ignore()).IgnoreAllNonExisting();
            CreateMap<UpdateExamPracticeSectionCommandModel, ExamPracticeSection>().ForMember(m => m.ExamPracticeSections, opt => opt.Ignore()).ForMember(m => m.Questions, opt => opt.Ignore()).ForMember(m => m.ExamPracticeAISettings, opt => opt.Ignore()).IgnoreAllNonExisting();
        }
    }
}
