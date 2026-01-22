// Copyright (c) Atlantic. All rights reserved.

using Fsel.Common.Helpers;
using Fsel.Course.Domain.Entities;
using Fsel.Course.Domain.Enums;
using Fsel.Shared.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fsel.Course.Infrastructure.Configs
{
    public class ExtraPracticeEntityTypeConfiguration : IEntityTypeConfiguration<ExtraPractice>
    {
        public void Configure(EntityTypeBuilder<ExtraPractice> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);
            builder.Property(e => e.Type)
                .HasMaxLength(100)
                .HasConversion(
                    v => v.ToString(),
                    v => v.EnumParse<EnumExtraPracticeType>());
            builder.Property(e => e.CourseLevel)
                .HasMaxLength(100)
                .HasConversion(
                    v => v.ToString(),
                    v => v.EnumParse<EnumCourseLevel>());

            builder.HasOne(a => a.Video)
                    .WithOne(b => b.ExtraPractice)
                    .HasForeignKey<ExtraPractice>(p => p.VideoId)
                    .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(a => a.PlacementTest)
                    .WithOne(b => b.ExtraPractice)
                    .HasForeignKey<ExtraPractice>(p => p.PlacementTestId)
                    .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(a => a.MockTest)
                   .WithOne(b => b.ExtraPractice)
                   .HasForeignKey<ExtraPractice>(p => p.MockTestId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
