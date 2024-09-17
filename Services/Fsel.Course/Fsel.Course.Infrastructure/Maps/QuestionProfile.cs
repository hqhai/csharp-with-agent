// Copyright (c) Atlantic. All rights reserved.

using AutoMapper;
using Fsel.Core.Extensions;
using Fsel.Course.Domain.Entities;
using Fsel.Course.Domain.Models.CommandModels.Questions;
using Fsel.Course.Domain.Models.EntityModels;

namespace Fsel.Course.Infrastructure.Maps
{
    public class QuestionProfile : Profile
    {
        public QuestionProfile()
        {
            CreateMap<Question, QuestionModel>()
                .ForMember(x => x.IsReportExplanation, x => x.MapFrom(n => n.QuestionExplanationErrors.Any()));
            CreateMap<CreateQuestionCommandModel, Question>().IgnoreAllNonExisting();
            CreateMap<Question, QuestionExplanationLogExportModel>().IgnoreAllNonExisting();
        }
    }
}
