// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Maps
{
    using AutoMapper;
    using Fsel.Core.Extensions;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Models.EntityModels.ArchiveModels;

    public class ArchiveProfile : Profile
    {
        public ArchiveProfile()
        {
            CreateMap<Course, CourseArchiveModel>().IgnoreAllNonExisting();
            CreateMap<Lesson, LessonArchiveModel>()
                .ForMember(x => x.TeacherId, x => x.MapFrom(n => n.LessonVideos.Select(m => m.Video).Select(d => d!.TeacherId).FirstOrDefault()))
                .ForMember(p => p.TimeCodeType, n => n.MapFrom(m => m.LessonVideos.Select(a => a.Video).Select(e => e!.VideoTimeCodes).FirstOrDefault()!.FirstOrDefault()!.TimeCodeType));
            CreateMap<CourseTeacher, TeacherModel>().IgnoreAllNonExisting();
            CreateMap<Unit, UnitArchiveModel>().ForMember(x => x.Teachers, x => x.MapFrom(n => n.CourseUnitMockTests.Select(cum => cum.Course).SelectMany(ct => ct!.CourseTeachers).DistinctBy(db => db.TeacherId)));
            CreateMap<Video, VideoArchiveModel>()
            .ForMember(x => x.Skills, x => x.MapFrom(p => p.VideoTimeCodes.Where(i => !i.IsDeleted).SelectMany(ve => ve.TimeCodeExercises).Select(e => e.Exercise).Select(sk => sk!.CourseSkill)));
            CreateMap<ExtraPractice, ExtraPracticeArchiveModel>().IgnoreAllNonExisting();
            CreateMap<MockTest, MockTestArchiveModel>()
                 .ForMember(x => x.Skills, x => x.MapFrom(mts => mts.MockTestSections.Select(sg => sg.SectionGroup).Select(sk => sk!.CourseSkill)));
            CreateMap<FinalTest, FinalTestArchiveModel>().IgnoreAllNonExisting();
            CreateMap<PlacementTest, PlacementTestArchiveModel>().IgnoreAllNonExisting();
            CreateMap<HomeWork, HomeworkArchiveModel>().IgnoreAllNonExisting();
        }
    }
}
