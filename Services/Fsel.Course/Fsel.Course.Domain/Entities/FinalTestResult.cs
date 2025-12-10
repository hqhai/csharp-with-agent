// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Entities
{
    using System.ComponentModel.DataAnnotations.Schema;
    using Fsel.Course.Domain.IEntities;

    public class FinalTestResult : BaseLearnResult, ITokenResult
    {
        [NotMapped]
        public override double PercentModule { get; set; }

        public FinalTest? FinalTest { get; set; }
        public Guid FinalTestId { get; set; }
        public Course? Course { get; set; }
        public Guid CourseId { get; set; }
        public int? TokenFirstTime { get; set; }
        public int? TokenLastTime { get; set; }
        public ICollection<FinalTestAnswer> FinalTestAnswers { get; set; } = new List<FinalTestAnswer>();
        public ICollection<SectionGroupResult> SectionGroupResults { get; set; } = new List<SectionGroupResult>();
    }
}
