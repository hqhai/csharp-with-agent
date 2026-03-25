// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Master.Domain.Entities
{
    using System.ComponentModel.DataAnnotations.Schema;
    using Microsoft.EntityFrameworkCore;

    [Keyless]
    [Table("Dim_Program", Schema = "dbo")]
    public class Program
    {
        public Guid ProgramId { get; set; }

        public string? ProgramCode { get; set; }

        public string? ProgramName { get; set; }

        public Guid SubjectId { get; set; }
    }
}
