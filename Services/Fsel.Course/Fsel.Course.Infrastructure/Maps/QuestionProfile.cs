using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fsel.Core.Extensions;
using Fsel.Course.Common.Models.Commands.Question;
using Fsel.Course.Domain.Entities;

namespace Fsel.Course.Infrastructure.Maps
{
    public class QuestionProfile : Profile
    {
        public QuestionProfile()
        {
            //CreateMap<Question, QuestionModel>().IgnoreAllNonExisting();
            CreateMap<CreateQuestionCommandModel, Question>().IgnoreAllNonExisting();
            CreateMap<UpdateQuestionCommandModel, Question>().IgnoreAllNonExisting();
        }
    }
}