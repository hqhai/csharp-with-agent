// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Training.Infrastructure.Configs
{
    using Fsel.Common.Helpers;
    using Fsel.Shared.Enums;
    using Fsel.Training.Domain.Entities;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class TeacherFreeTimeEntityTypeConfiguration : IEntityTypeConfiguration<TeacherFreeTime>
    {
        public void Configure(EntityTypeBuilder<TeacherFreeTime> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);
            builder.HasOne(a => a.TeacherFreeDate)
               .WithMany(b => b.TeacherFreeTimes)
               .HasForeignKey(b => b.TeacherFreeDateId)
               .OnDelete(DeleteBehavior.Cascade);

            builder.Property(e => e.DayOfWeek)
                .HasMaxLength(100)
                .HasConversion(
                    v => v.ToString(),
                    v => v.EnumParse<EnumDayOfWeek>());
        }
    }
}
