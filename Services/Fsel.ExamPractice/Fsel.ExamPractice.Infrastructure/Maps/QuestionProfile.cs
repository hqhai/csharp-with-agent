// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.ExamPractice.Infrastructure.Maps
{
    using AutoMapper;
    using Fsel.Core.Extensions;
    using Fsel.ExamPractice.Domain.Entities;
    using Fsel.ExamPractice.Domain.Models.CommandModels.Questions;
    using Fsel.ExamPractice.Domain.Models.EntityModels.Questions;

    public class QuestionProfile : Profile
    {
        public QuestionProfile()
        {
            CreateMap<Question, Question>().IgnoreAllNonExisting();
            CreateMap<Question, QuestionModel>().IgnoreAllNonExisting();
            CreateMap<CreateQuestionCommandModel, Question>().IgnoreAllNonExisting();
            CreateMap<UpdateQuestionCommandModel, Question>().IgnoreAllNonExisting();
        }
    }
}
