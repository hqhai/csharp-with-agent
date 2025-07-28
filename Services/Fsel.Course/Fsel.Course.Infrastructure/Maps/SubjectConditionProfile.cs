// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Maps
{
    using AutoMapper;
    using Fsel.Core.Extensions;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Models.CommandModels.SubjectConditions;
    using Fsel.Course.Domain.Models.EntityModels;

    public class SubjectConditionProfile : Profile
    {
        public SubjectConditionProfile()
        {
            CreateMap<SubjectCondition, SubjectConditionModel>().IgnoreAllNonExisting();
            CreateMap<CreateSubjectConditionCommandModel, SubjectCondition>().ForMember(x => x.SubjectConditionRules, x => x.Ignore());
            CreateMap<UpdateSubjectConditionCommandModel, SubjectCondition>().ForMember(x => x.SubjectConditionRules, x => x.Ignore());
        }
    }
}
