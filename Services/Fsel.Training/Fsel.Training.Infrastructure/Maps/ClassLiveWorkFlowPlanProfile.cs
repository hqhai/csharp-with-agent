// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Training.Infrastructure.Maps
{
    using AutoMapper;
    using Fsel.Core.Extensions;
    using Fsel.Training.Domain.Entities;
    using Fsel.Training.Domain.Models.CommandModels.ClassLiveWorkFlowPlans;
    using Fsel.Training.Domain.Models.EntityModels;

    public class ClassLiveWorkFlowPlanProfile : Profile
    {
        public ClassLiveWorkFlowPlanProfile()
        {
            CreateMap<ClassLiveWorkFlowPlan, ClassLiveWorkFlowPlanModel>().IgnoreAllNonExisting();
            CreateMap<CreateClassLiveWorkFlowPlanCommandModel, ClassLiveWorkFlowPlan>().IgnoreAllNonExisting();
            CreateMap<ClassLiveWorkFlow, ChangeLiveSessionInfoModel>().IgnoreAllNonExisting();
        }
    }
}
