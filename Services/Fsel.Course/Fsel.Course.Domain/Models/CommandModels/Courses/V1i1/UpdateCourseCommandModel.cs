// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.CommandModels.Courses.V1i1
{
    using Fsel.Core.Base.BaseModels;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.Models.CommandModels.CourseTeachers;

    public class UpdateCourseCommandModel : BaseCommandModel
    {
        public string? Name { get; set; }

        public string? Code { get; set; }

        public string? InstructionContent { get; set; }

        public Guid LevelId { get; set; }

        public Guid ProgramId { get; set; }

        public IList<UpdateCourseModuleModel>? Modules { get; set; }

        public IList<CreateCourseTeacherCommandModel>? CourseTeachers { get; set; }
    }

    public class UpdateCourseModuleModel
    {
        public EnumCourseConfigType CourseConfigType { get; set; }

        public double Percent { get; set; }

        public int OpenOrder { get; set; }

        public Guid OriginalId { get; set; }
    }
}
