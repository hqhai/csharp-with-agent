// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    using Fsel.Shared.Enums;

    public class OverallReportModel
    {
        public EnumCourseLevel CourseLevel { get; set; }
        public int OverallScore { get; set; }
        public IList<OverallSkillReportModel>? OverallSkillReports { get; set; }
        public OverallClassForumReportModel? Receive { get; set; }
        public OverallClassForumReportModel? Give { get; set; }
    }

    public class OverallSkillReportModel
    {
        public EnumCourseSkill Skill { get; set; }
        public int Value { get; set; }
    }

    public class OverallClassForumReportModel
    {
        public int LikeNumber { get; set; }
        public int CommentNumber { get; set; }
    }
}
