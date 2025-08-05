// Copyright (c) Atlantic. All rights reserved.

using AutoMapper;
using Fsel.Core.Extensions;
using Fsel.Course.Domain.Entities;
using Fsel.Course.Domain.Models.CommandModels.Lessons;
using Fsel.Course.Domain.Models.EntityModels;

namespace Fsel.Course.Infrastructure.Maps
{
    public class LessonProfile : Profile
    {
        public LessonProfile()
        {
            CreateMap<Lesson, LessonOverviewModel>().IgnoreAllNonExisting();
            CreateMap<Lesson, LessonHistoryModel>().IgnoreAllNonExisting();
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
        }
    }
}
