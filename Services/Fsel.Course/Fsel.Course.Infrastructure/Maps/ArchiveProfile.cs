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
            CreateMap<Course, ArchiveCourseModel>().IgnoreAllNonExisting();
            CreateMap<Lesson, ArchiveLessonModel>()
                .ForMember(x => x.TeacherId, x => x.MapFrom(n => n.LessonVideos.Select(m => m.Video).Select(d => d!.TeacherId).FirstOrDefault()))
                .ForMember(p => p.TimeCodeType, n => n.MapFrom(m => m.LessonVideos.Select(a => a.Video).Select(e => e!.VideoTimeCodes).FirstOrDefault()!.FirstOrDefault()!.TimeCodeType));
            CreateMap<CourseTeacher, TeacherModel>().IgnoreAllNonExisting();
            CreateMap<Unit, ArchiveUnitModel>().ForMember(x => x.Teachers, x => x.MapFrom(n => n.CourseUnitMockTests.FirstOrDefault()!.Course!.CourseTeachers.Where(p => !p.IsDeleted)));
            CreateMap<LessonVideo, ArchiveVideoLessonModel>().ForMember(x => x.Name, x => x.MapFrom(p => p.Video!.Name))
            .ForMember(x => x.Skills, x => x.MapFrom(p => p.Video!.VideoTimeCodes.Where(i => !i.IsDeleted).SelectMany(ve => ve.TimeCodeExercises.Where(isd => !isd.IsDeleted)).Select(e => e.Exercise).Select(sk => sk!.CourseSkill)));
        }
    }
}
