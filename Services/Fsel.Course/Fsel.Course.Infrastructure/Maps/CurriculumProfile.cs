// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Maps
{
    using AutoMapper;
    using Fsel.Core.Extensions;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Models.CommandModels.Curriculums;
    using Fsel.Course.Domain.Models.EntityModels;

    public class CurriculumProfile : Profile
    {
        public CurriculumProfile()
        {
            CreateMap<CreateCurriculumCommandModel, CurriculumConfig>().IgnoreAllNonExisting();
            CreateMap<UpdateCurriculumCommandModel, CurriculumConfig>().IgnoreAllNonExisting();
            CreateMap<CurriculumConfig, CurriculumModel>().IgnoreAllNonExisting();
        }
    }
}
