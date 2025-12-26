// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Common.CourseHelpers
{
    using Fsel.Common.Enums;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Entities.V1i1;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.Models.CommandModels.Courses.V1i1;
    using Fsel.Course.Domain.Models.CommandModels.CourseTeachers;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Helpers;

    public class CourseFactory
    {
        private readonly UpdateCourseCommandModel _createRequest;

        protected CourseFactory(UpdateCourseCommandModel createRequest)
        {
            _createRequest = createRequest;
        }

        public Course Build(int version = 0, Guid? originalId = null)
        {
            var course = new Course
            {
                Code = _createRequest.Code,
                Name = _createRequest.Name,
                InstructionContent = _createRequest.InstructionContent,
                UnitCount = _createRequest.Modules?.Count(m => m.CourseConfigType == EnumCourseConfigType.Unit) ?? default,
                TestCount = _createRequest.Modules?.Count(m => m.CourseConfigType == EnumCourseConfigType.Test) ?? default,
                VersionStatus = EnumVersionStatus.LastVersion,
                Version = version,
                LevelId = _createRequest.LevelId,
                ProgramId = _createRequest.ProgramId,
                Status = _createRequest.Status
            };
            course.OriginalId = originalId.HasValue ? originalId.Value : course.Id;

            course.CourseModules = CourseModuleClassification(_createRequest.Modules).ToList();
            course.CourseTeachers = CourseTeacherClassification(_createRequest.CourseTeachers).ToList();

            return course;
        }

        private static IEnumerable<CourseModule> CourseModuleClassification(IList<UpdateCourseModuleModel>? modules)
        {
            if (modules != null)
            {
                for (var i = 0; i < modules.Count; i++)
                {
                    var module = modules[i];
                    yield return new CourseModule
                    {
                        CourseConfigType = module.CourseConfigType,
                        Percent = module.Percent,
                        OpenOrder = module.OpenOrder,
                        OriginalId = module.OriginalId,
                        DisplayOrder = i + 1,
                        DisplayNumber = modules.IndexOfSubSet(module, m => m.CourseConfigType == module.CourseConfigType) + 1 ?? 0
                    };
                }
            }
        }

        private static IEnumerable<CourseTeacher> CourseTeacherClassification(IList<CreateCourseTeacherCommandModel>? courseTeachers)
        {
            if (courseTeachers != null)
            {
                foreach (var courseTeacher in courseTeachers)
                {
                    yield return new CourseTeacher
                    {
                        TeacherId = courseTeacher.TeacherId,
                        AvatarPath = courseTeacher.AvatarPath,
                        Nationality = courseTeacher.Nationality,
                        Deggree = courseTeacher.Deggree,
                        Experience = courseTeacher.Experience,
                        Strength = courseTeacher.Strength
                    };
                }
            }
        }

        public static CourseFactory Create(UpdateCourseCommandModel createRequest)
        {
            return new CourseFactory(createRequest);
        }
    }
}
