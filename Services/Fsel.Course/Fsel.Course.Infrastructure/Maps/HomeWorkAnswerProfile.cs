// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Maps
{
    using AutoMapper;
    using Fsel.Core.Extensions;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Models.EntityModels;

    public class HomeWorkAnswerProfile : Profile
    {
        public HomeWorkAnswerProfile()
        {
            CreateMap<HomeWorkAnswer, HomeWorkAnswerModel>().IgnoreAllNonExisting();
            CreateMap<HomeWorkAnswer, AnswerModel>().IgnoreAllNonExisting();
            CreateMap<HomeWorkExtraPracticeAnswer, AnswerModel>().IgnoreAllNonExisting();
        }
    }
}
