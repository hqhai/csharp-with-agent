// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.ExamPractice.Infrastructure.Maps
{
    using AutoMapper;
    using Fsel.Core.Extensions;
    using Fsel.ExamPractice.Domain.Entities;
    using Fsel.ExamPractice.Domain.Models.CommandModels.ExamPractices;
    using Fsel.ExamPractice.Domain.Models.EntityModels.ExamPractices;

    public class ExamPracticeProfile : Profile
    {
        public ExamPracticeProfile()
        {
            CreateMap<ExamPractice, ExamPracticeModel>().IgnoreAllNonExisting();
            CreateMap<ExamPractice, ExamPracticeSearchModel>().IgnoreAllNonExisting();
            CreateMap<ExamPractice, ExamPracticeDetailModel>().IgnoreAllNonExisting();
            CreateMap<CreateExamPracticeCommandModel, ExamPractice>().ForMember(m => m.ExamPracticeSections, opt => opt.Ignore()).IgnoreAllNonExisting();
            CreateMap<UpdateExamPracticeCommandModel, ExamPractice>().ForMember(m => m.ExamPracticeSections, opt => opt.Ignore()).IgnoreAllNonExisting();
        }
    }
}
