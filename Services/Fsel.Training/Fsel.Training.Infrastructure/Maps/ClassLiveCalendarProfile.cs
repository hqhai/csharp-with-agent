// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Training.Infrastructure.Maps
{
    using AutoMapper;
    using Fsel.Core.Extensions;
    using Fsel.Training.Domain.Entities;
    using Fsel.Training.Domain.Models.CommandModels.Classes;
    using Fsel.Training.Domain.Models.EntityModels;

    public class ClassLiveCalendarProfile : Profile
    {
        public ClassLiveCalendarProfile()
        {
            CreateMap<ClassLiveCalendar, ClassLiveCalendarModel>().IgnoreAllNonExisting();
            CreateMap<SaveClassLiveCsoCommandModel, ClassLiveCalendar>().IgnoreAllNonExisting();
        }
    }
}
