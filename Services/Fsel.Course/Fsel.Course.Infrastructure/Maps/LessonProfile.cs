// Copyright (c) Atlantic. All rights reserved.

using AutoMapper;
using Fsel.Core.Extensions;
using Fsel.Course.Domain.Entities;
using Fsel.Course.Domain.Models.CommandModels.Lessons;
using Fsel.Course.Domain.Models.EntityModels;
using Fsel.Course.Domain.Models.EntityModels.DashboardModels;

namespace Fsel.Course.Infrastructure.Maps
{
    public class LessonProfile : Profile
    {
        public LessonProfile()
        {
            CreateMap<Lesson, LessonOverviewModel>().IgnoreAllNonExisting();
            CreateMap<Lesson, HomeNavigationTargetModel>().IgnoreAllNonExisting();
            CreateMap<Lesson, LessonModel>().IgnoreAllNonExisting();
            CreateMap<CreateLessonCommandModel, Lesson>().IgnoreAllNonExisting();
            CreateMap<UpdateLessonCommandModel, Lesson>().IgnoreAllNonExisting();
        }
    }
}
