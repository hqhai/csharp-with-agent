// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Maps
{
    using AutoMapper;
    using Fsel.Core.Extensions;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Entities.SubjectConditionRuleConfigs;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.Models.CommandModels.SubjectConditionRules;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Domain.Models.EntityModels.SubjectConditionRuleConfigModels;

    public class SubjectConditionRuleProfile : Profile
    {
        public SubjectConditionRuleProfile()
        {
            CreateMap<SubjectConditionRule, SubjectConditionRuleModel>().IgnoreAllNonExisting();
            CreateMap<CreateSubjectConditionRuleCommandModel, SubjectConditionRule>().IgnoreAllNonExisting();
            CreateMap<UpdateSubjectConditionRuleCommandModel, SubjectConditionRule>().IgnoreAllNonExisting();
            CreateMap<ConditionRule, ConditionRuleModel>().AfterMap((src, dest) =>
            {
                if (src.Type == EnumSubjectConditionRuleType.CurrentLevel && src.LevelIds != null && src.LevelIds.Any())
                {
                    dest.Levels = src.LevelIds.Select(x => new BaseProgramLevelDetailModel
                    {
                        LevelId = x
                    }).ToList();
                }
            });

            CreateMap<ConditionValue, ConditionValueModel>().AfterMap((src, dest) =>
            {
                if (src.Type != EnumSubjectConditionValueType.Maximum && src.LevelIds != null && src.LevelIds.Any())
                {
                    dest.Levels = src.LevelIds.Select(x => new BaseProgramLevelDetailModel
                    {
                        LevelId = x
                    }).ToList();
                }
            });
        }
    }
}
