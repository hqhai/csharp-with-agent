// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Infrastructure.Configs
{
    using Fsel.System.Domain.Entities.CourseGoals;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class CourseGoalConfigEntityTypeConfiguration : IEntityTypeConfiguration<CourseGoalConfig>
    {
        public void Configure(EntityTypeBuilder<CourseGoalConfig> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);
            builder.HasOne(a => a.CourseGoal)
                   .WithMany(b => b.CourseGoalConfigs)
                   .HasForeignKey(b => b.CourseGoalId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
