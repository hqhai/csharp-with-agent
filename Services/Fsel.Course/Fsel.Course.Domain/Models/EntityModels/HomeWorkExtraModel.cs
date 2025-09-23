// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    using Fsel.Shared.Enums;

    public class HomeWorkExtraModel
    {
        public Guid Id { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string? Name { get; set; }
        public EnumCourseLevel CourseLevel { get; set; }
        public EnumCourseSkill CourseSkill { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int CorrectCount { get; set; }
        public int CorrectTotal { get; set; }
        public string? TopicName { get; set; }
        public HomeWorkExtraPracticeResultModel? HomeWorkExtraPracticeResult { get; set; }
    }
}
