// Copyright (c) Atlantic. All rights reserved.

using AutoMapper;
using Fsel.Core.Extensions;
using Fsel.Course.Domain.Entities;
using Fsel.Course.Domain.Entities.V1i1;
using Fsel.Course.Domain.Enums;
using Fsel.Course.Domain.Models.CommandModels.Units;
using Fsel.Course.Domain.Models.EntityModels;
using Fsel.Shared.Helpers;

namespace Fsel.Course.Infrastructure.Maps
{
    public class UnitProfile : Profile
    {
        public UnitProfile()
        {
            CreateMap<CreateUnitCommandModel, Unit>().IgnoreAllNonExisting();
            CreateMap<UpdateUnitCommandModel, Unit>().IgnoreAllNonExisting();
            CreateMap<UnitResult, UnitResultModel>().ForMember(x => x.ProgressPercent, p => p.MapFrom(x =>
                x.Unit != null ?
                    x.Unit.UnitSkillMockTests.Any() ?
                    NumberHelper.GetPercent(x.Unit.LessonResults.Where(y => y.Status == EnumResultStatus.Done).Count() + x.Unit.UnitSkillMockTests.Select(x => x.MockTest).Where(x => x!.MockTestResults.Any()).SelectMany(x => x!.MockTestResults).Count(y => y.UnitId == x.UnitId && y.Status == EnumResultStatus.Done), x.Unit.UnitLessons.Count + x.Unit.UnitSkillMockTests.Count, 0)
                    : NumberHelper.GetPercent(x.Unit.LessonResults.Where(y => y.Status == EnumResultStatus.Done).Count(), x.Unit.UnitLessons.Count, 0)
                : default
            ));
            CreateMap<Unit, UnitModel>().ForMember(x => x.IsActive, p => p.MapFrom(o => o.CourseUnitMockTests.Any()));

            CreateMap<UnitResult, CourseUnitMockTestResultModel>().IgnoreAllNonExisting();

            CreateMap<UnitModule, UnitModuleDTO>().IgnoreAllNonExisting();
        }
    }
}
