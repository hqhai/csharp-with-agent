// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Maps
{
    using AutoMapper;
    using Fsel.Core.Extensions;
    using Fsel.Course.Domain.Entities.FlowConfigs;
    using Fsel.Course.Domain.Models.CommandModels.Flows;
    using Fsel.Course.Domain.Models.EntityModels.FlowModels;

    public class FlowProfile : Profile
    {
        public FlowProfile()
        {
            CreateMap<Flow, FlowModel>().IgnoreAllNonExisting();
            CreateMap<SaveFlowCommandModel, Flow>()
                .ForMember(m => m.StepFlows, opt => opt.Ignore())
                .ForMember(m => m.Id, opt => opt.Ignore());

            CreateMap<StepFlow, StepFlowModel>()
                .ForMember(x => x.LevelName, x => x.MapFrom(n => n.Level != null ? n.Level.Name : string.Empty));
            CreateMap<SaveStepFlowCommandModel, StepFlow>()
                .ForMember(m => m.ChildActionFlows, opt => opt.Ignore())
                .ForMember(m => m.Id, opt => opt.Ignore());

            CreateMap<ActionFlow, ActionFlowModel>().IgnoreAllNonExisting();
            CreateMap<SaveActionFlowCommandModel, ActionFlow>().ForMember(m => m.Id, opt => opt.Ignore());
        }
    }
}
