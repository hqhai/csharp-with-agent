// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Infrastructure.Configs
{
    using System;
    using Fsel.Identity.Domain.Entities;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class StudentDailyStreakEntityTypeConfiguration : IEntityTypeConfiguration<StudentDailyStreak>
    {
        public void Configure(EntityTypeBuilder<StudentDailyStreak> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);

            builder.HasOne(a => a.Student)
                 .WithMany(b => b.StudentDailyStreaks)
                 .HasForeignKey(p => p.StudentId)
                 .OnDelete(DeleteBehavior.NoAction);

            builder.HasIndex(c => new { c.IsDeleted, c.StudentId });
        }
    }
}
