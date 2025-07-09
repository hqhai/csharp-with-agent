// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Infrastructure.Maps
{
    using AutoMapper;
    using Fsel.Core.Extensions;
    using Fsel.Interaction.Domain.Entities;
    using Fsel.Interaction.Domain.Models.CommandModels.SurveyQuestions;
    using Fsel.Interaction.Domain.Models.EntityModels;

    public class SurveyQuestionProfile : Profile
    {
        public SurveyQuestionProfile()
        {
            CreateMap<SurveyQuestionTranslation, SurveyQuestion>().IgnoreEntity()?.ReverseMap();
            CreateMap<SurveyQuestionTranslation, SurveyQuestionTranslationModel>().IgnoreAllNonExisting()?.ReverseMap();
            CreateMap<SurveyQuestion, SurveyQuestionModel>().IgnoreAllNonExisting()?.MapTranslations<SurveyQuestion, SurveyQuestionModel, SurveyQuestionTranslation>();
            CreateMap<CreateSurveyQuestionCommandModel, SurveyQuestion>().IgnoreAllNonExisting();
        }
    }
}
