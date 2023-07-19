// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Infrastructure.Configs
{
    using Fsel.Common.Helpers;
    using Fsel.Interaction.Domain.Entities;
    using Fsel.Shared.Enums;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class StudentReviewEntityTypeConfiguration : IEntityTypeConfiguration<StudentReview>
    {
        public void Configure(EntityTypeBuilder<StudentReview> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);
            builder.Property(e => e.ReviewType)
                .HasMaxLength(100)
                .HasConversion(
                    v => v.ToString(),
                    v => v.EnumParse<EnumReviewType>());
        }
    }
}
