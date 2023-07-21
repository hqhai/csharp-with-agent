// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Infrastructure.Maps
{
    using AutoMapper;
    using Fsel.Core.Extensions;
    using Fsel.Interaction.Domain.Entities;
    using Fsel.Interaction.Domain.Models.CommandModels.SupportCategorys;
    using Fsel.Interaction.Domain.Models.EntityModels;

    public class SupportCategoryProfile : Profile
    {
        public SupportCategoryProfile()
        {
            CreateMap<SupportCategory, SupportCategoryModel>().IgnoreAllNonExisting();
            CreateMap<CreateSupportCategoryCommandModel, SupportCategory>().IgnoreAllNonExisting();
            CreateMap<UpdateSupportCategoryCommandModel, SupportCategory>().IgnoreAllNonExisting();
        }
    }
}
