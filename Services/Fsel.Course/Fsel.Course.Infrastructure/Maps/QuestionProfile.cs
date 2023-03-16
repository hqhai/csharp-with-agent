using AutoMapper;
using Fsel.Core.Extensions;
using Fsel.Course.Domain.Entities;
using Fsel.Course.Domain.Models.CommandModels.Questions;

namespace Fsel.Course.Infrastructure.Maps
{
    public class QuestionProfile : Profile
    {
        public QuestionProfile()
        {
            CreateMap<CreateQuestionCommandModel, Question>().IgnoreAllNonExisting();
            CreateMap<UpdateQuestionCommandModel, Question>().IgnoreAllNonExisting();
        }
    }
}
