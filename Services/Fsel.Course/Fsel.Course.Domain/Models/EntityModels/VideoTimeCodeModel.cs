// Copyright (c) Atlantic. All rights reserved.

using Fsel.Course.Domain.Enums;
using Fsel.Shared.Enums;

namespace Fsel.Course.Domain.Models.EntityModels
{
    public class VideoTimeCodeModel
    {
        public Guid Id { get; set; }
        public int TotalCount { get; set; }
        public bool Ungraded { get; set; }
        public int CorrectTotal { get; set; }
        public int CorrectCount { get; set; }
        public EnumTimeCodeType TimeCodeType { get; set; }
        public EnumResultStatus Status { get; set; }
        public Guid VideoId { get; set; }
        public double DisplayTime { get; set; }
        public double ExecutionTime { get; set; }
        public IList<ExerciseModel> Exercises { get; set; } = new List<ExerciseModel>();
        public IList<EnumCourseSkill>? CourseSkills { get; set; }
        public IList<SkillViewModel>? Skills { get; set; }
        public VideoTimeCodeResultModel? VideoTimeCodeResult { get; set; }
    }
}
