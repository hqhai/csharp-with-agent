// Copyright (c) Atlantic. All rights reserved.

using Fsel.Common.Helpers;
using Fsel.Course.Domain.Entities;
using Fsel.Course.Domain.Enums;
using Fsel.Shared.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fsel.Course.Infrastructure.Configs
{
    public class HomeWorkEntityTypeConfiguration : IEntityTypeConfiguration<HomeWork>
    {
        public void Configure(EntityTypeBuilder<HomeWork> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);
            builder.Property(e => e.CourseLevel)
                .HasMaxLength(20)
                .HasConversion(
                    v => v.ToString(),
                    v => v.EnumParse<EnumCourseLevel>());

            builder.Property(e => e.CourseSkill)
                .HasMaxLength(20)
                .HasConversion(
                    v => v.ToString(),
                    v => v.EnumParse<EnumCourseSkill>());

            builder.Property(e => e.Type)
               .HasMaxLength(20)
               .HasConversion(
                   v => v.ToString(),
                   v => v.EnumParse<EnumHomeWorkType>());

            builder.HasOne(a => a.Topic)
                   .WithMany(b => b.HomeWorks)
                   .HasForeignKey(b => b.TopicId)
                   .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
