// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Infrastructure.Maps
{
    using AutoMapper;
    using Fsel.Core.Extensions;
    using Fsel.Interaction.Domain.Entities;
    using Fsel.Interaction.Domain.Models.EntityModels;

    public class CustomerSurveyProfile : Profile
    {
        public CustomerSurveyProfile()
        {
            CreateMap<CustomerSurvey, CustomerSurveyModel>().IgnoreAllNonExisting();
        }
    }
}
