// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Training.Infrastructure.Configs
{
    using Fsel.Common.Helpers;
    using Fsel.Training.Domain.Entities;
    using Fsel.Training.Domain.Enums;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class ClassLiveWorkFlowEntityTypeConfiguration : IEntityTypeConfiguration<ClassLiveWorkFlow>
    {
        public void Configure(EntityTypeBuilder<ClassLiveWorkFlow> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);

            builder.Property(e => e.Status)
                .HasMaxLength(100)
                .HasConversion(
                    v => v.ToString(),
                    v => v.EnumParse<EnumWorkFlowStatus>());

            builder.Property(e => e.Type)
               .HasMaxLength(100)
               .HasConversion(
                   v => v.ToString(),
                   v => v.EnumParse<EnumWorkFlow>());

            builder.HasOne(a => a.ClassLiveCalendar)
                     .WithMany(b => b.ClassLiveWorkFlows)
                     .HasForeignKey(b => b.ClassLiveCalendarId)
                     .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
