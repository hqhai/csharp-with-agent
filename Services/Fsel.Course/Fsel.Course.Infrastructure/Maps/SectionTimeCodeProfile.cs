// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Maps
{
    using AutoMapper;
    using Fsel.Core.Extensions;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Models.CommandModels.SectionTimeCodes;
    using Fsel.Course.Domain.Models.EntityModels;

    public class SectionTimeCodeProfile : Profile
    {
        public SectionTimeCodeProfile()
        {
            CreateMap<SectionTimeCode, SectionTimeCodeModel>()
                .ForMember(p => p.Answer, x => x.MapFrom(n => n.MockTestAnswers.Select(x => x.Answer).FirstOrDefault() ?? n.ExtraPracticeAnswers.Select(x => x.Answer).FirstOrDefault() ?? default));
            CreateMap<CreateSectionTimeCodeCommandModel, SectionTimeCode>().IgnoreAllNonExisting();
            CreateMap<UpdateSectionTimeCodeCommandModel, SectionTimeCode>();
            CreateMap<SectionTimeCode, SectionTimeCodeDtoModel>()
                .ForMember(p => p.Answer, x => x.MapFrom(n => n.MockTestAnswers.Select(x => x.Answer).FirstOrDefault() ?? n.ExtraPracticeAnswers.Select(x => x.Answer).FirstOrDefault() ?? default));
        }
    }
}
