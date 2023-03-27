// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Configs
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Fsel.Common.Helpers;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Enums;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class UnitLessonResultEntityTypeConfiguraion : IEntityTypeConfiguration<UnitLessonResult>
    {
        public void Configure(EntityTypeBuilder<UnitLessonResult> builder)
        {
            builder.HasOne(a => a.UnitLesson)
                .WithMany(b => b.UnitLessonResults)
                .HasForeignKey(b => b.UnitLessonId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Property(e => e.Status)
                .HasMaxLength(100)
                .HasConversion(
                    v => v.ToString(),
                    v => v.EnumParse<EnumResultStatus>());
        }
    }
}
