// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Master.Domain.Entities
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("StudentCompetitionEvent_Report", Schema = "dbo")]
    public class StudentCompetitionEvent
    {
        [Key]
        public Guid Id { get; set; }

        public Guid StudentId { get; set; }

        public Guid CompetitionEventId { get; set; }

        public DateTime? CreatedDate { get; set; }
    }
}
