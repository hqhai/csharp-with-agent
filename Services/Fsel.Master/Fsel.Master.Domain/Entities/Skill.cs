// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Master.Domain.Entities
{
    using System.ComponentModel.DataAnnotations.Schema;
    using Microsoft.EntityFrameworkCore;

    [Keyless]
    [Table("Dim_Skill", Schema = "dbo")]
    public class Skill
    {
        public Guid SkillId { get; set; }
        public string? SkillCode { get; set; }
        public string? SkillName { get; set; }
    }
}
