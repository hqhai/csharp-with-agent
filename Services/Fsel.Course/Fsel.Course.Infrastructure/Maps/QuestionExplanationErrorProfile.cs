// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Maps
{
    using AutoMapper;
    using Fsel.Core.Extensions;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Models.CommandModels.QuestionExplanationErrors;
    using Fsel.Course.Domain.Models.EntityModels;

    public class QuestionExplanationErrorProfile : Profile
    {
        public QuestionExplanationErrorProfile()
        {
            CreateMap<CreateQuestionExplanationErrorCommandModel, QuestionExplanationError>().IgnoreAllNonExisting();
            CreateMap<QuestionExplanationError, QuestionExplanationErrorExportModel>().IgnoreAllNonExisting();
        }
    }
}
