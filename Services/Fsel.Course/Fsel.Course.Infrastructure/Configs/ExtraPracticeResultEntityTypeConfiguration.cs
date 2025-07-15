// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Configs
{
    using System;
    using Fsel.Common.Helpers;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Enums;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class ExtraPracticeResultEntityTypeConfiguration : IEntityTypeConfiguration<ExtraPracticeResult>
    {
        public void Configure(EntityTypeBuilder<ExtraPracticeResult> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);
            builder.Property(e => e.Status)
               .HasMaxLength(20)
               .HasConversion(
                   v => v.ToString(),
                   v => v.EnumParse<EnumResultStatus>());

            builder.HasOne(a => a.ExtraPractice)
                .WithMany(b => b.ExtraPracticeResults)
                .HasForeignKey(b => b.ExtraPracticeId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
