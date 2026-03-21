// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Master.Domain.Entities
{
    using System;
    using System.ComponentModel.DataAnnotations.Schema;
    using Fsel.Master.Domain.Models.Enums;
    using Microsoft.EntityFrameworkCore;

    [Keyless]
    [Table("PlacementTest_Report", Schema = "dbo")]
    public class PlacementTestReport
    {
        public Guid? TestGroupResultId { get; set; }

        public Guid StudentId { get; set; }

        public Guid? LevelId { get; set; }

        public Guid? ProgramId { get; set; }

        public EnumResultStatus? Status { get; set; }

        public decimal? ScorePercent { get; set; }

        public int? TotalQuestions { get; set; }

        public int? CorrectCount { get; set; }

        public DateTime? CompletionDate { get; set; }
    }
}
