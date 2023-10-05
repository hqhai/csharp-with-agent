// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Entities
{
    using System.ComponentModel.DataAnnotations;
    using Fsel.Common.Enums.ErrorCodes;

    public class FinalTestResult : BaseResultScore
    {
        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        public Guid StudentId { get; set; }

        public FinalTest? FinalTest { get; set; }
        public Guid FinalTestId { get; set; }
        public Course? Course { get; set; }
        public Guid CourseId { get; set; }
        public ICollection<FinalTestAnswer> FinalTestAnswers { get; set; } = new List<FinalTestAnswer>();
    }
}
