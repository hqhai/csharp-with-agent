// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Entities
{
    public class FinalTestResult : BaseLearnResult
    {
        public FinalTest? FinalTest { get; set; }
        public Guid FinalTestId { get; set; }
        public Course? Course { get; set; }
        public Guid CourseId { get; set; }
        public ICollection<FinalTestAnswer> FinalTestAnswers { get; set; } = new List<FinalTestAnswer>();
        public ICollection<SectionGroupResult> SectionGroupResults { get; set; } = new List<SectionGroupResult>();
    }
}
