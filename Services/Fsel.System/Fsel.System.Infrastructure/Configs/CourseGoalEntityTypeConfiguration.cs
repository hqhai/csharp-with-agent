// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Infrastructure.Configs
{
    using Fsel.Common.Helpers;
    using Fsel.Shared.Enums;
    using Fsel.System.Domain.Entities.CourseGoals;
    using Fsel.System.Domain.Enums;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class CourseGoalEntityTypeConfiguration : IEntityTypeConfiguration<CourseGoal>
    {
        public void Configure(EntityTypeBuilder<CourseGoal> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);

            builder.Property(e => e.CourseType)
                    .HasMaxLength(100)
                    .HasConversion(v => v.ToString(),
                    v => v.EnumParse<EnumCourseType>());

            builder.Property(e => e.CourseLevel)
                   .HasMaxLength(100)
                   .HasConversion(v => v.ToString(),
                   v => v.EnumParse<EnumCourseLevel>());

            builder.Property(e => e.GoalCategory)
                  .HasMaxLength(100)
                  .HasConversion(v => v.ToString(),
                  v => v.EnumParse<EnumCourseGoalCategory>());
        }
    }
}
