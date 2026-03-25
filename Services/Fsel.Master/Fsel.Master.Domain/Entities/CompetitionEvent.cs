// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Master.Domain.Entities
{
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Text.Json;
    using Microsoft.EntityFrameworkCore;

    [Keyless]
    [Table("Dim_CompetitionEvent", Schema = "dbo")]
    public class CompetitionEvent
    {
        public Guid CompetitionEventId { get; set; }

        public string? EventCode { get; set; }

        public string? Name { get; set; }

        public string? Category { get; set; }

        public Guid? LocationId { get; set; }

        public Guid? ParentEventId { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public string? SchoolIdsJson { get; set; }

        [NotMapped]
        public IList<Guid>? SchoolIds
        {
            get
            {
                return !string.IsNullOrEmpty(SchoolIdsJson) ? JsonSerializer.Deserialize<IList<Guid>>(SchoolIdsJson) : null;
            }
        }
    }
}
