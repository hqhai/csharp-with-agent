// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Infrastructure.Maps
{
    using AutoMapper;
    using Fsel.Core.Extensions;
    using Fsel.Identity.Domain.Entities.Campus;
    using Fsel.Identity.Domain.Models.CommandModels.Campus;
    using Fsel.Identity.Domain.Models.EntityModels;

    public class SchoolClassProfile : Profile
    {
        public SchoolClassProfile()
        {
            CreateMap<SaveSchoolClassCommandModel, SchoolClass>().IgnoreAllNonExisting();
            CreateMap<SchoolClass, SchoolClassModel>().IgnoreAllNonExisting();
        }
    }
}
