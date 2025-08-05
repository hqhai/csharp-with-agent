// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Configs
{
    using Fsel.Common.Helpers;
    using Fsel.Course.Domain.Entities.V1i1;
    using Fsel.Course.Domain.Enums;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class CourseModuleEntityTypeConfiguration : IEntityTypeConfiguration<CourseModule>
    {
        public void Configure(EntityTypeBuilder<CourseModule> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);

            builder.Property(e => e.CourseConfigType)
                   .HasMaxLength(100)
                   .HasConversion(
                       v => v.ToString(),
                       v => v.EnumParse<EnumCourseConfigType>());

            builder.HasOne(x => x.Course)
                   .WithMany(x => x.CourseModules)
                   .HasForeignKey(x => x.CourseId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
