// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Training.Infrastructure.Maps
{
    using AutoMapper;
    using Fsel.Core.Extensions;
    using Fsel.Training.Domain.Entities;
    using Fsel.Training.Domain.Models.CommandModels.Classes;
    using Fsel.Training.Domain.Models.CommandModels.TeacherFreeDates;
    using Fsel.Training.Domain.Models.CommandModels.TeacherFreeTimes;
    using Fsel.Training.Domain.Models.EntityModels;

    public class TeacherFreeDateProfile : Profile
    {
        public TeacherFreeDateProfile()
        {
            CreateMap<TeacherFreeDate, TeacherFreeDateModel>().IgnoreAllNonExisting();
            CreateMap<TeacherFreeTime, TeacherFreeTimeModel>().IgnoreAllNonExisting();
            CreateMap<CreateTeacherFreeDateCommandModel, TeacherFreeDate>().IgnoreAllNonExisting();
            CreateMap<CreateTeacherFreeTimeCommandModel, TeacherFreeTime>().IgnoreAllNonExisting();
            CreateMap<AssignTeacherToClassModel, TeacherFreeTime>().IgnoreAllNonExisting();
        }
    }
}
