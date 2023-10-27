// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Infrastructure.Configs
{
    using Fsel.Common.Helpers;
    using Fsel.Interaction.Domain.Entities;
    using Fsel.Shared.Enums;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class StudentReviewDetailEntityTypeConfiguration : IEntityTypeConfiguration<StudentReviewDetail>
    {
        public void Configure(EntityTypeBuilder<StudentReviewDetail> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);
            builder.Property(e => e.ReviewQuestionType)
                .HasMaxLength(100)
                .HasConversion(
                    v => v.ToString(),
                    v => v.EnumParse<EnumReviewQuestionType>());

            builder.HasOne(a => a.StudentReview)
                  .WithMany(b => b.StudentReviewDetails)
                  .HasForeignKey(b => b.StudentReviewId)
                  .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
