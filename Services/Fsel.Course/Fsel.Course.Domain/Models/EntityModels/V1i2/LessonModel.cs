// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels.V1i2
{
    using Shared.Enums;
    using V1i1;

    public class LessonModel : BaseScoreResultModel
    {
        public string? Name { get; set; }

        public string? InstructionContent { get; set; }

        public Guid? LevelId { get; set; }

        public string? Description { get; set; }

        public string? Thumbnail { get; set; }

        public string? CourseLevel { get; set; }

        public Guid CourseId { get; set; }

        public Guid UnitId { get; set; }

        public Guid ObjectId { get; set; }

        public string? Type { get; set; }

        public bool? IsCheckScoreColor { get; set; }

        public double? TargetBandScore { get; set; }

        public bool? IsTeacherGraded { get; set; }

        public EnumCourseSkill? CourseSkill { get; set; }

        public double? Scores { get; set; }
        public List<LessonModuleModel> LessonModules { get; set; } = new();
    }
}
