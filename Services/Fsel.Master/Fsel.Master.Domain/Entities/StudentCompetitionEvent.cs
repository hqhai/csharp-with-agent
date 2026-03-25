// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Master.Domain.Entities
{
    using System.ComponentModel.DataAnnotations.Schema;
    using Microsoft.EntityFrameworkCore;

    [Keyless]
    [Table("StudentCompetitionEvent_Report", Schema = "dbo")]
    public class StudentCompetitionEvent
    {
        public Guid Id { get; set; }

        public Guid StudentId { get; set; }

        public Guid CompetitionEventId { get; set; }

        public DateTime? CreatedDate { get; set; }
    }
}
