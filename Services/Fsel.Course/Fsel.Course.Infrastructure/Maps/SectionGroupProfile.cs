// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Maps
{
    using AutoMapper;
    using Fsel.Core.Extensions;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Models.CommandModels.SectionGroups;
    using Fsel.Course.Domain.Models.EntityModels;

    public class SectionGroupProfile : Profile
    {
        public SectionGroupProfile()
        {
            CreateMap<SectionGroup, SectionGroupModel>().IgnoreAllNonExisting();
            CreateMap<CreateSectionGroupCommandModel, SectionGroup>().IgnoreAllNonExisting();
            CreateMap<UpdateSectionGroupCommandModel, SectionGroup>().IgnoreAllNonExisting();

            CreateMap<SectionQuestion, SectionQuestionModel>().IgnoreAllNonExisting();
        }
    }
}
