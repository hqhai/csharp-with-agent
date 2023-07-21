// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Infrastructure.Maps
{
    using AutoMapper;
    using Fsel.Core.Extensions;
    using Fsel.Interaction.Domain.Entities;
    using Fsel.Interaction.Domain.Models.CommandModels.SupportQuestions;
    using Fsel.Interaction.Domain.Models.EntityModels;

    public class SupportQuestionProfile : Profile
    {
        public SupportQuestionProfile()
        {
            CreateMap<SupportQuestion, SupportQuestionModel>().IgnoreAllNonExisting();
            CreateMap<CreateSupportQuestionCommandModel, SupportQuestion>().IgnoreAllNonExisting();
            CreateMap<UpdateSupportQuestionCommandModel, SupportQuestion>().IgnoreAllNonExisting();
        }
    }
}
