// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Master.Domain.Entities
{
    using System;
    using System.ComponentModel.DataAnnotations.Schema;
    using Fsel.Shared.Enums;
    using Microsoft.EntityFrameworkCore;

    [Keyless]
    [Table("LearningActivity_Report", Schema = "dbo")]
    public class LearningActivity
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid CourseId { get; set; }
        public Guid StudentId { get; set; }
        public EnumFeature Feature { get; set; }
        public int VisitCount { get; set; }
        public int AccessTime { get; set; }
        public DateTime? LastVisited { get; set; }
        public string? UserAgent { get; set; }
        public DateTime? CreatedDate { get; set; }
    }
}
