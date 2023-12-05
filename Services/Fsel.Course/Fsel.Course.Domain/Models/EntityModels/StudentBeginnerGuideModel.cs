// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    using Fsel.Shared.Enums;

    public class StudentBeginnerGuideModel
    {
        public bool? CourseOverview { get; set; }
        public bool? TeacherOverView { get; set; }
        public bool? ViewUnit { get; set; }
        public bool? ViewLessonOverview { get; set; }
        public bool? VideoLesson { get; set; }
        public bool? ClassForum { get; set; }
        public bool? HomeWork { get; set; }
        public bool? PlacementTest { get; set; }
        public IList<EnumQuestionType>? QuestionTypes { get; set; }
    }
}
