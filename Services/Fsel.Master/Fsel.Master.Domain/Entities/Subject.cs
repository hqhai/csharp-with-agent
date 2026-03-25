// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Master.Domain.Entities
{
    using System.ComponentModel.DataAnnotations.Schema;
    using Microsoft.EntityFrameworkCore;

    [Keyless]
    [Table("Dim_Subject", Schema = "dbo")]
    public class Subject
    {
        public Guid SubjectId { get; set; }

        public string? SubjectCode { get; set; }

        public string? SubjectName { get; set; }
    }
}
