// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Entities
{
    using System.ComponentModel.DataAnnotations;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Course.Domain.IEntities;
    using Fsel.Shared.Enums;

    public class CourseResult : BaseScoreResult, IModuleLifeCycle
    {
        [Range(0, 100, ErrorMessage = nameof(EnumSystemErrorCode.Min))]
        public override double Percent { get; set; }

        public Course? Course { get; set; }
        public Guid CourseId { get; set; }
        public EnumWorkingStatus WorkingStatus { get; set; }
        public DateTime? NewDate { get; set; }
        public DateTime? ProcessDate { get; set; }
        public DateTime? CompletionDate { get; set; }
    }
}
