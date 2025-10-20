// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Infrastructure.Configs
{
    using System;
    using Fsel.Common.Helpers;
    using Fsel.Interaction.Domain.Entities;
    using Fsel.Shared.Enums;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class UserSurveyAssignmentEntityTypeConfiguration : IEntityTypeConfiguration<UserSurveyAssignment>
    {
        public void Configure(EntityTypeBuilder<UserSurveyAssignment> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);

            builder.Property(e => e.CourseType)
             .HasMaxLength(100)
             .HasConversion(
                 v => v == null ? null : v.ToString(),
                 v => string.IsNullOrEmpty(v) ? null : v.EnumParse<EnumCourseType>());

            builder.Property(e => e.CourseLevel)
             .HasMaxLength(100)
             .HasConversion(
                 v => v == null ? null : v.ToString(),
                 v => string.IsNullOrEmpty(v) ? null : v.EnumParse<EnumCourseLevel>());

            builder.Property(e => e.ProgressRequirement)
             .HasMaxLength(100)
             .HasConversion(
                 v => v == null ? null : v.ToString(),
                 v => string.IsNullOrEmpty(v) ? null : v.EnumParse<EnumProgressRequirement>());

            builder.HasOne(a => a.SurveyConfig)
                  .WithMany(b => b.UserSurveyAssignments)
                  .HasForeignKey(p => p.SurveyConfigId)
                  .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
