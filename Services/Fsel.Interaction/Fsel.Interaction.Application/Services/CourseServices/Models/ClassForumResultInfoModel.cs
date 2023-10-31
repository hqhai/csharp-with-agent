// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Application.Services.CourseServices.Models
{
    using Fsel.Core.Base.BaseModels;

    public class ClassForumResultInfoModel: BaseModel
    {
        public string? Content { get; set; }
        public string? WordContent { get; set; }
        public string? GradingAlFeedback { get; set; }
        public Guid? GradingTeacherId { get; set; }
        public Guid LessonResultId { get; set; }
        public Guid StudentId { get; set; }
        public Guid ClassForumId { get; set; }
        public Guid? CheckCsoId { get; set; }
        public DateTime? CheckStartDate { get; set; }
        public DateTime? GradingStartDate { get; set; }
        public Guid CourseId { get; set; }
        public Guid UnitId { get; set; }
    }
}
