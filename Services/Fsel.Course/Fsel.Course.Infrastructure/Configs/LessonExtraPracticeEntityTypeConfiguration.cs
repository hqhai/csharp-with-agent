// Copyright (c) Atlantic. All rights reserved.

using Fsel.Course.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fsel.Course.Infrastructure.Configs
{
    public class LessonExtraPracticeEntityTypeConfiguration : IEntityTypeConfiguration<LessonExtraPractice>
    {
        public void Configure(EntityTypeBuilder<LessonExtraPractice> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);

            builder.HasOne(a => a.Lesson)
                            .WithMany(b => b.LessonExtraPractices)
                            .HasForeignKey(b => b.LessonId)
                            .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(a => a.ExtraPractice)
                .WithMany(b => b.LessonExtraPractices)
                .HasForeignKey(b => b.ExtracPraticeId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
