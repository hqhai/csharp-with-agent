// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Configs
{
    using Fsel.Common.Helpers;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Enums;
    using Fsel.Shared.Enums;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class PlacementTestGroupResultEntityTypeConfiguration : IEntityTypeConfiguration<PlacementTestGroupResult>
    {
        public void Configure(EntityTypeBuilder<PlacementTestGroupResult> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);
            builder.Property(e => e.CompletionLevel)
                .HasMaxLength(100)
                .HasConversion(
                    v => v.ToString(),
                    v => v.EnumParse<EnumPlacementTestLevel>());

            builder.Property(e => e.ProcessLevel)
                .HasMaxLength(100)
                .HasConversion(
                    v => v.ToString(),
                    v => v.EnumParse<EnumPlacementTestLevel>());

            builder.Property(e => e.Status)
             .HasMaxLength(100)
             .HasConversion(
                 v => v.ToString(),
                 v => v.EnumParse<EnumResultStatus>());

            builder.Property(e => e.SuggetLevel)
                .HasMaxLength(100)
                .HasConversion(
                    v => v.ToString(),
                    v => v.EnumParse<EnumCourseLevel>());

            builder.Property(e => e.CurrentLevel)
                   .HasMaxLength(100)
                   .HasConversion(
                       v => v.ToString(),
                       v => v.EnumParse<EnumCourseLevel>());

            builder.Property(e => e.ChooseLevel)
                   .HasMaxLength(100)
                   .HasConversion(
                    v => v.ToString(),
                    v => v.EnumParse<EnumCourseLevel>());

            builder.HasIndex(c => c.StudentId).IsUnique();

            builder.HasIndex(x => new { x.CreatedDate }).IncludeValueProperties(x => new { x.StudentId });
            builder.HasIndex(x => new { x.IsDeleted, x.Status }).IncludeValueProperties(x => new { x.SuggetLevel, x.StudentId });
        }
    }
}
