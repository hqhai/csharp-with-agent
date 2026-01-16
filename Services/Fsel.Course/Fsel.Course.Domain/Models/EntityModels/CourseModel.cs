// Copyright (c) Atlantic. All rights reserved.

using Fsel.Core.Base.BaseModels;
using Fsel.Shared.Enums;

namespace Fsel.Course.Domain.Models.EntityModels
{
    public class CourseModel : BaseModel
    {
        public string? Name { get; set; }

        public string? Code { get; set; }

        public string? InstructionContent { get; set; }

        public EnumCourseStatus Status { get; set; }

        public EnumCourseLevel CourseLevel { get; set; }

        public EnumCourseType CourseType { get; set; }

        public int UnitCount { get; set; }

        public int TestCount { get; set; }

        public double Percent { get; set; }

        public int Version { get; set; }

        public Guid? LevelId { get; set; }

        public string? LevelName { get; set; }

        public Guid? ProgramId { get; set; }

        public string? ProgramName { get; set; }

        public Guid OriginalId { get; set; }

        public bool IsUsed { get; set; }

        public IList<CourseUnitMockTestModel>? CourseUnitMockTests { get; set; }
        public IList<CourseTeacherModel>? CourseTeachers { get; set; }
        public IList<CourseModuleModel>? CourseModules { get; set; }
        public CourseResultModel? CourseResult { get; set; }
        public CourseClassModel? CourseClass { get; set; }
    }
}
