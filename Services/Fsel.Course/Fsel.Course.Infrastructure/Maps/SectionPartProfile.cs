// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Maps
{
    using AutoMapper;
    using Fsel.Core.Extensions;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Models.CommandModels.SectionParts;
    using Fsel.Course.Domain.Models.EntityModels;

    public class SectionPartProfile : Profile
    {
        public SectionPartProfile()
        {
            CreateMap<SectionPart, SectionPartModel>().IgnoreAllNonExisting();
            CreateMap<CreateSectionPartCommandModel, SectionPart>().IgnoreAllNonExisting();
            CreateMap<UpdateSectionPartCommandModel, SectionPart>().IgnoreAllNonExisting();
        }
    }
}
