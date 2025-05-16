// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.ExamPractice.Infrastructure.Maps
{
    using AutoMapper;
    using Fsel.Core.Extensions;
    using Fsel.ExamPractice.Domain.Entities;
    using Fsel.ExamPractice.Domain.Models.CommandModels.ExamPracticeSections;
    using Fsel.ExamPractice.Domain.Models.EntityModels;

    public class ExamPracticeSectionProfile : Profile
    {
        public ExamPracticeSectionProfile()
        {
            CreateMap<ExamPracticeSection, ExamPracticeSectionModel>().IgnoreAllNonExisting();
            CreateMap<CreateExamPracticeSectionCommandModel, ExamPracticeSection>().ForMember(m => m.ExamPracticeSections, opt => opt.Ignore()).IgnoreAllNonExisting();
            CreateMap<UpdateExamPracticeSectionCommandModel, ExamPracticeSection>().ForMember(m => m.ExamPracticeSections, opt => opt.Ignore()).IgnoreAllNonExisting();
        }
    }
}
