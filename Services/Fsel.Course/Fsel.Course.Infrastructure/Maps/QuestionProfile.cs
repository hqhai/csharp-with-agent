// Copyright (c) Atlantic. All rights reserved.

using AutoMapper;
using Fsel.Core.Extensions;
using Fsel.Course.Domain.Entities;
using Fsel.Course.Domain.Models.CommandModels.Questions;
using Fsel.Course.Domain.Models.EntityModels;
using Fsel.Shared.Enums;

namespace Fsel.Course.Infrastructure.Maps
{
    public class QuestionProfile : Profile
    {
        public QuestionProfile()
        {
            CreateMap<Question, QuestionModel>().IgnoreAllNonExisting();
            CreateMap<CreateQuestionCommandModel, Question>().IgnoreAllNonExisting();
            CreateMap<UpdateQuestionCommandModel, Question>().IgnoreAllNonExisting();
            CreateMap<UpdateQuestionCommandModel, CreateQuestionCommandModel>().ForMember(p => p.Id, x => x.Ignore()).IgnoreAllNonExisting();
            CreateMap<Question, QuestionExplanationLogExportModel>().IgnoreAllNonExisting();
        }
    }
}
