// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Infrastructure.Configs
{
    using Fsel.Common.Helpers;
    using Fsel.Shared.Enums;
    using Fsel.System.Domain.Entities;
    using global::System;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class FeatureAccessTimeConfigConfiguration : IEntityTypeConfiguration<FeatureAccessTime>
    {
        public void Configure(EntityTypeBuilder<FeatureAccessTime> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);
            builder.Property(e => e.EnumFeature)
                .HasMaxLength(100)
                .HasConversion(
                    v => v.ToString(),
                    v => v.EnumParse<EnumFeature>());

            builder.HasIndex(c => new { c.CreatedUserId, c.IsDeleted, c.EnumFeature });
            builder.HasIndex(c => new { c.CreatedUserId, c.IsDeleted, c.CourseId, c.UnitId, c.ObjectId });
            builder.HasIndex(c => new { c.IsDeleted }).IncludeValueProperties(x => new { x.CreatedUserId, x.AccessTime });
        }
    }
}
