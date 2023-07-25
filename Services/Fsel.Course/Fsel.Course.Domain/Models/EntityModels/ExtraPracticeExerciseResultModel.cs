// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    using System;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Course.Domain.Enums;
    using Fsel.Shared.Enums;

    public class ExtraPracticeResultModel : BaseModel
    {
        public double Percent { get; set; }
        public int CorrectCount { get; set; }
        public int CorrectTotal { get; set; }
        public int ExecuteCount { get; set; }
        public EnumResultStatus Status { get; set; }
        public EnumCourseSkill? CourseSkill { get; set; }
        public Guid StudentId { get; set; }
        public Guid ExtraPracticeExerciseId { get; set; }
    }
}
