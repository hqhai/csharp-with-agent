// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Shared.Models.SenderTemplates
{
    using Fsel.Shared.Enums;

    public class SendStudentCompleteMidCourseModel
    {
        public string? FullName { get; set; }
        public string? CourseLevel { get; set; }
        public string? StartDate { get; set; }
        public string? EndDate { get; set; }
        public string? Percent { get; set; }
        public string? Unit1Name { get; set; }
        public string? UnitNowNumber { get; set; }
        public string? UnitNowName { get; set; }
        public string? TotalLesson { get; set; }
        public string? TotalDay { get; set; }
        public string? TotalNote { get; set; }
        public string? Video { get; set; }
        public string? Homework { get; set; }
        public string? ClassForum { get; set; }
        public string? SkillTest { get; set; }
        public string? UnitTest { get; set; }
        public string? BandScore { get; set; }
        public string? Level { get; set; }
        public string? Description { get; set; }
        public string? ColorCircle { get; set; }
        public string? ContinueLearn { get; set; }
        public string? LinkReport { get; set; }
        public string? HideSkillTest { get; set; }
        public string? IndexMiddleUnit { get; set; }
        public string? TotalUnit { get; set; }
        public EnumCourseType CourseType { get; set; }
        public string? AccessLink { get; set; }
    }
}
