// Copyright (c) Atlantic. All rights reserved.

using Fsel.Common.Helpers;
using Fsel.Course.Domain.Entities;
using Fsel.Course.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fsel.Course.Infrastructure.Configs
{
    public class VideoTimeCodeResultEntityTypeConfiguration : IEntityTypeConfiguration<VideoTimeCodeResult>
    {
        public void Configure(EntityTypeBuilder<VideoTimeCodeResult> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);
            builder.Property(e => e.Status)
                  .HasMaxLength(20)
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

            builder.HasIndex(c => new { c.Status, c.StudentId }).IncludeValueProperties(x => new { x.VideoResultId, x.VideoTimeCodeId, x.CorrectCount, x.CorrectTotal, x.WorkingTime });
            builder.HasIndex(c => new { c.VideoResultId, c.Status });
            builder.HasIndex(c => new { c.VideoResultId, c.VideoTimeCodeId, c.IsDeleted });

            builder.Property(x => x.Percent)
                   .HasComputedColumnSql(@"CASE WHEN ([CorrectTotal] + [CorrectTotalUngraded]) > 0 THEN ROUND(([CorrectCount] + [CorrectCountUngraded] * 100.0) / ([CorrectTotal] + [CorrectTotalUngraded]), 0) ELSE 0 END", stored: true)
                   .ValueGeneratedOnAddOrUpdate()
                   .Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);
        }
    }
}
