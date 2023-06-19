// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Training.Infrastructure.Configs
{
    using Fsel.Training.Domain.Entities;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class ClassLiveCalendarEntityTypeConfiguaration : IEntityTypeConfiguration<ClassLiveCalendar>
    {
        public void Configure(EntityTypeBuilder<ClassLiveCalendar> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);
            builder.HasOne(a => a.Class)
               .WithMany(b => b.ClassLiveCalendars)
               .HasForeignKey(b => b.ClassId)
               .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
