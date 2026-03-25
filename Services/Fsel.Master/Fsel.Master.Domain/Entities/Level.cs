// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Master.Domain.Entities
{
    using System;
    using System.ComponentModel.DataAnnotations.Schema;
    using Microsoft.EntityFrameworkCore;

    [Keyless]
    [Table("Dim_Level", Schema = "dbo")]
    public class Level
    {
        public Guid LevelId { get; set; }

        public string? LevelCode { get; set; }

        public string? LevelName { get; set; }

        public Guid ProgramId { get; set; }

        public int? LevelOrder { get; set; }
    }
}
