// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Maps
{
    using AutoMapper;
    using Fsel.Core.Extensions;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Models.CommandModels.QuestionForms;
    using Fsel.Course.Domain.Models.EntityModels;

    public class QuestionFormProfile : Profile
    {
        public QuestionFormProfile()
        {
            CreateMap<QuestionForm, QuestionFormModel>().IgnoreAllNonExisting();
            CreateMap<CreateQuestionFormCommandModel, QuestionForm>().IgnoreAllNonExisting();
        }
    }
}
