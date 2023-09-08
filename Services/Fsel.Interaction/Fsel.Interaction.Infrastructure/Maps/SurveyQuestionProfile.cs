// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Infrastructure.Maps
{
    using AutoMapper;
    using Fsel.Interaction.Domain.Entities;
    using Fsel.Core.Extensions;
    using Fsel.Interaction.Domain.Models.EntityModels;

    public class SurveyQuestionProfile : Profile
    {
        public SurveyQuestionProfile()
        {
            CreateMap<SurveyQuestion, SurveyQuestionModel>().IgnoreAllNonExisting();
        }
    }
}
