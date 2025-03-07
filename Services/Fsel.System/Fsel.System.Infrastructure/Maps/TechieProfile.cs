// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Infrastructure.Maps
{
    using AutoMapper;
    using Fsel.Core.Extensions;
    using Fsel.System.Domain.Entities;
    using Fsel.System.Domain.Models.CommandModels.Techie;
    using Fsel.System.Domain.Models.EntityModels;

    public class TechieProfile : Profile
    {
        public TechieProfile()
        {
            CreateMap<SaveTechieActionCommandModel, TechieAction>().IgnoreAllNonExisting();
            CreateMap<TechieAction, TechieActionModel>().IgnoreAllNonExisting();

            CreateMap<TechieActionTranslation, TechieAction>().IgnoreEntity()?.ReverseMap();
            CreateMap<TechieActionTranslation, TechieActionTranslation>().IgnoreAllNonExisting()?.ReverseMap();
            CreateMap<TechieAction, TechieActionModel>().IgnoreAllNonExisting()?.MapTranslations<TechieAction, TechieActionModel, TechieActionTranslation>();
        }
    }
}
