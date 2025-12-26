// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Maps
{
    using AutoMapper;
    using Core.Extensions;
    using Domain.Entities;
    using Domain.Models.EntityModels;

    public class StatusStudentGoalHistoryProfile : Profile
    {
        public StatusStudentGoalHistoryProfile()
        {
            CreateMap<StatusStudentGoalHistory, StausStudentGoalHistoryModel>().IgnoreAllNonExisting();
            CreateMap<StausStudentGoalHistoryModel, StatusStudentGoalHistory>().IgnoreAllNonExisting();
        }
    }
}
