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
            CreateMap<Lesson, LessonHistoryModel>().IgnoreAllNonExisting();
            CreateMap<Lesson, HomeNavigationTargetModel>().IgnoreAllNonExisting();
            CreateMap<Lesson, LessonModel>().IgnoreAllNonExisting();
            CreateMap<CreateLessonCommandModel, Lesson>().IgnoreAllNonExisting();
            CreateMap<UpdateLessonCommandModel, Lesson>().IgnoreAllNonExisting();
            CreateMap<Lesson, Fsel.Course.Domain.Models.EntityModels.V1i1.LessonModel>()
                    .ForMember(p => p.NameLevel, x => x.MapFrom(n => n.Level != null ? n.Level.Name : null))
                    .ForMember(p => p.NameProgram, x => x.MapFrom(n => n.Category != null ? n.Category.Name : null));
            CreateMap<Fsel.Course.Domain.Models.CommandModels.Lessons.V1i1.CreateLessonCommandModel, Lesson>()
                    .ForMember(p => p.LessonModules, x => x.Ignore())
                    .ForMember(p => p.LessonInstructions, x => x.Ignore());
            CreateMap<Fsel.Course.Domain.Models.CommandModels.Lessons.V1i1.UpdateLessonCommandModel, Lesson>()
                    .ForMember(p => p.LessonModules, x => x.Ignore())
                    .ForMember(p => p.LessonInstructions, x => x.Ignore());

            CreateMap<Lesson, Domain.Models.EntityModels.V1i2.LessonDtoModel>().IgnoreAllNonExisting();

            CreateMap<Lesson, Fsel.Course.Domain.Models.EntityModels.V1i2.LessonModel>()
                .ForMember(d => d.CourseLevel,
                    o => o.MapFrom(s => s.CourseLevel.ToString()))
                .ForMember(d => d.UnitId,
                    o => o.Ignore())
                .ForMember(d => d.ObjectId,
                    o => o.MapFrom(s => s.OriginalId))
                .ForMember(d => d.Status,
                    o => o.Ignore())
                .ForMember(d => d.DisplayOrder,
                    o => o.Ignore())
                .ForMember(d => d.IsLocked,
                    o => o.Ignore())
                .ForMember(d => d.LessonModules,
                    o => o.Ignore());
            ;
        }
    }
}
