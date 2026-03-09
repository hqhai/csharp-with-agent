// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Configs
{
    using System;
    using Fsel.Common.Helpers;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Enums;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class DocumentResultEntityTypeConfiguration : IEntityTypeConfiguration<DocumentResult>
    {
        public void Configure(EntityTypeBuilder<DocumentResult> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);

            builder.Property(e => e.Status)
                   .HasMaxLength(30)
                   .HasConversion(
                       v => v.ToString(),
                       v => v.EnumParse<EnumResultStatus>());

            builder.HasOne(a => a.Document)
                  .WithMany(b => b.DocumentResults)
                  .HasForeignKey(p => p.DocumentId)
                  .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(a => a.LessonModule)
                  .WithMany(b => b.DocumentResults)
                  .HasForeignKey(p => p.LessonModuleId)
                  .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(a => a.LessonResult)
                  .WithMany(b => b.DocumentResults)
                  .HasForeignKey(p => p.LessonResultId)
                  .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(c => new { c.LessonResultId, c.LessonModuleId }).IsUnique().HasFilter("[IsDeleted] = 0");
        }
    }
}
