// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Training.Infrastructure.Configs
{
    using System;
    using Fsel.Training.Domain.Entities;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class TeacherFreeTimeLiveEntityTypeConfiguration : IEntityTypeConfiguration<TeacherFreeTimeLive>
    {
        public void Configure(EntityTypeBuilder<TeacherFreeTimeLive> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);
            builder.HasOne(a => a.TeacherFreeTime)
               .WithMany(b => b.TeacherFreeTimeLives)
               .HasForeignKey(b => b.TeacherFreeTimeId)
               .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
