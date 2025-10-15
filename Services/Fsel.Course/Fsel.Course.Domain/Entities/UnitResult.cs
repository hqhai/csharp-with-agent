// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Entities
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Course.Domain.Entities.TestConfigs;
    using Fsel.Course.Domain.IEntities;

    public class UnitResult : BaseScoreResult, IModuleLifeCycle
    {
        public Course? Course { get; set; }

        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        public Guid CourseId { get; set; }

        [Range(0, 100, ErrorMessage = nameof(EnumSystemErrorCode.Min))]
        public override double Percent { get; set; }

        public DateTime? ProcessDate { get; set; }
        public DateTime? CompletionDate { get; set; }
        public Unit? Unit { get; set; }

        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        public Guid UnitId { get; set; }

        public ICollection<TestGroupResult> TestGroupResults { get; set; } = new List<TestGroupResult>();
    }
}
