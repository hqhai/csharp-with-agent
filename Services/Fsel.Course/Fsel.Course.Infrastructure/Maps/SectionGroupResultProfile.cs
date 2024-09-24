// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Maps
{
    using AutoMapper;
    using Fsel.Core.Extensions;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Models.EntityModels;

    public class SectionGroupResultProfile : Profile
    {
        public SectionGroupResultProfile()
        {
            CreateMap<SectionGroupResult, SectionGroupResultModel>().ForMember(p => p.RemainingTime, x => x.MapFrom(n => n.SectionGroup != null && n.SectionGroup.ExecutionTime - n.WorkingTime > 0 ? n.SectionGroup.ExecutionTime - n.WorkingTime : default));
        }
    }
}
