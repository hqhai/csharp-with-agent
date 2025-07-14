// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Configs
{
    using System;
    using Fsel.Common.Helpers;
    using Fsel.Course.Domain.Entities.V1i1;
    using Fsel.Course.Domain.Enums;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class LessonModuleEntityTypeConfiguration : IEntityTypeConfiguration<LessonModule>
    {
        public void Configure(EntityTypeBuilder<LessonModule> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);

            builder.Property(e => e.LessonConfigType)
                   .HasMaxLength(20)
                   .HasConversion(
                       v => v.ToString(),
                       v => v.EnumParse<EnumLessonConfigType>());

            builder.HasOne(a => a.Lesson)
                   .WithMany(b => b.LessonModules)
                   .HasForeignKey(p => p.LessonId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
