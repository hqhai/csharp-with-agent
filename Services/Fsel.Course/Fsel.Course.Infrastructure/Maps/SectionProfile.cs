// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Maps
{
    using AutoMapper;
    using Fsel.Core.Extensions;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Models.CommandModels.Sections;
    using Fsel.Course.Domain.Models.EntityModels;

    public class SectionProfile : Profile
    {
        public SectionProfile()
        {
            CreateMap<Section, SectionModel>().IgnoreAllNonExisting();
            CreateMap<CreateSectionCommandModel, Section>().IgnoreAllNonExisting();
            CreateMap<UpdateSectionCommandModel, Section>().IgnoreAllNonExisting();
            CreateMap<Section, SectionDtoModel>()
                .ForMember(p => p.Answer, x => x.MapFrom(n => n.MockTestAnswers.Select(x => x.Answer).FirstOrDefault() ?? n.ExtraPracticeAnswers.Select(x => x.Answer).FirstOrDefault() ?? default))
                .ForMember(p => p.QuestionIds, x => x.MapFrom(n => n.SectionQuestions.OrderBy(x => x.CreatedDate).Where(x => x.QuestionId.HasValue).Select(x => x.QuestionId!.Value).ToList()));
        }
    }
}
