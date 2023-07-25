// Copyright (c) Atlantic. All rights reserved.

using AutoMapper;
using Fsel.Core.Extensions;
using Fsel.Course.Domain.Entities;
using Fsel.Course.Domain.Models.CommandModels.ExtraPractices;
using Fsel.Course.Domain.Models.EntityModels;

namespace Fsel.Course.Infrastructure.Maps
{
    public class ExtraPracticeProfile : Profile
    {
        public ExtraPracticeProfile()
        {
            CreateMap<ExtraPractice, ExtraPracticeModel>().IgnoreAllNonExisting();
            CreateMap<CreateExtraPracticeCommandModel, ExtraPractice>().IgnoreAllNonExisting();
            CreateMap<ExtraPracticeExercise, ExtraPracticeExerciseModel>().IgnoreAllNonExisting();
            CreateMap<ExtraPracticeExerciseResult, ExtraPracticeResultModel>().IgnoreAllNonExisting();
            CreateMap<ExtraPracticeAnswer, ExtraPracticeAnswerModel>().IgnoreAllNonExisting();
            CreateMap<UpdateExtraPracticeCommandModel, ExtraPractice>().ForMember(m => m.Video, opt => opt.Ignore()).IgnoreAllNonExisting();
            CreateMap<ExtraPracticeAnswer, AnswerModel>().IgnoreAllNonExisting();

        }
    }
}
