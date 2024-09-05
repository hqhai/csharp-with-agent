// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    public class FeatureModuleModel
    {
        public Guid? CourseId { get; set; }
        public Guid? CourseResultId { get; set; }
        public Guid? UnitId { get; set; }
        public Guid? UnitResultId { get; set; }
        public Guid? FinalTestId { get; set; }
        public Guid? FinalTestResultId { get; set; }
        public Guid? LessonId { get; set; }
        public Guid? LessonResultId { get; set; }
        public Guid? MockTestId { get; set; }
        public Guid? MockTestResultId { get; set; }
        public Guid? VideoId { get; set; }
        public Guid? VideoResultId { get; set; }
        public Guid? ClassForumId { get; set; }
        public Guid? ClassForumResultId { get; set; }
        public Guid? HomeWorkId { get; set; }
        public Guid? HomeWorkResultId { get; set; }
        public Guid? SectionGroupId { get; set; }
        public Guid? SectionGroupResultId { get; set; }
        public Guid StudentId { get; set; }
    }
}
