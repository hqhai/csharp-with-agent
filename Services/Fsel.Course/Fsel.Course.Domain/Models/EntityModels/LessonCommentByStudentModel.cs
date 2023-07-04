// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    using Fsel.Core.Base.BaseModels;

    public class LessonCommentByStudentModel : BaseModel
    {
        public string? UnitName { get; set; }
        public string? LessonName { get; set; }
        public double NumberOfStars { get; set; }
        public string? Feedback { get; set; }
    }
}
