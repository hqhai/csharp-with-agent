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
            CreateMap<SectionTimeCode, SectionTimeCodeModel>().IgnoreAllNonExisting();
            CreateMap<SectionTimeCode, SectionTimeCodeDetailModel>().IgnoreAllNonExisting();
            CreateMap<CreateSectionTimeCodeCommandModel, SectionTimeCode>().IgnoreAllNonExisting();
            CreateMap<UpdateSectionTimeCodeCommandModel, SectionTimeCode>().IgnoreAllNonExisting();
        }
    }
}
