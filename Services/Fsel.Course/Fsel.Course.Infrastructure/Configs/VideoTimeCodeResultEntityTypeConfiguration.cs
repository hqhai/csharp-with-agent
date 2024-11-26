// Copyright (c) Atlantic. All rights reserved.

using Fsel.Common.Helpers;
using Fsel.Course.Domain.Entities;
using Fsel.Course.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fsel.Course.Infrastructure.Configs
{
    public class VideoTimeCodeResultEntityTypeConfiguration : IEntityTypeConfiguration<VideoTimeCodeResult>
    {
        public void Configure(EntityTypeBuilder<VideoTimeCodeResult> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);
            builder.Property(e => e.Status)
                  .HasMaxLength(100)
                  .HasConversion(
                      v => v.ToString(),
                      v => v.EnumParse<EnumResultStatus>());

            builder.HasOne(a => a.VideoTimeCode)
                  .WithMany(b => b.VideoTimeCodeResults)
                  .HasForeignKey(b => b.VideoTimeCodeId)
                  .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(a => a.VideoResult)
                   .WithMany(b => b.VideoTimeCodeResults)
                   .HasForeignKey(b => b.VideoResultId)
                   .OnDelete(DeleteBehavior.NoAction);

            builder.HasIndex(c => new { c.VideoResultId, c.VideoTimeCodeId, c.StudentId }).IsUnique();

            builder.HasAnnotation("SqlServer:RawSqlIndex",
                @"CREATE INDEX IX_VideoTimeCodeResults_Status_StudentId_WithInclude
                ON [VideoTimeCodeResults] ([Status], [StudentId])
                INCLUDE ([VideoResultId], [VideoTimeCodeId], [CorrectCount], [CorrectTotal], [WorkingTime])");
        }
    }
}
