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
            CreateMap<ExamPractice, ExamPracticeReportModel>().IgnoreAllNonExisting();
            CreateMap<ExamPractice, ExamPracticeGroupModel>()
                .ForMember(p => p.IsNew, x => x.MapFrom(n => n.ActivatedAt.HasValue && DateTime.UtcNow <= n.ActivatedAt.Value.AddDays(7)))
                .ForMember(p => p.ExamPracticeStatus, x => x.MapFrom(n => n.Status));
            CreateMap<ExamPractice, ExamPracticeSearchModel>().IgnoreAllNonExisting();
            CreateMap<ExamPractice, ExamPracticeDetailModel>().IgnoreAllNonExisting();
            CreateMap<CreateExamPracticeCommandModel, ExamPractice>().ForMember(m => m.ExamPracticeSections, opt => opt.Ignore()).IgnoreAllNonExisting();
            CreateMap<UpdateExamPracticeCommandModel, ExamPractice>().ForMember(m => m.ExamPracticeSections, opt => opt.Ignore()).ForMember(m => m.Id, opt => opt.Ignore()).IgnoreAllNonExisting();
        }
    }
}
