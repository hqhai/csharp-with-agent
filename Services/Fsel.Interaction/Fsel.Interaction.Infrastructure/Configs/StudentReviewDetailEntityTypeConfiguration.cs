// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Infrastructure.Configs
{
    using Fsel.Interaction.Domain.Entities;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;
    using Microsoft.EntityFrameworkCore;
    using Fsel.Common.Helpers;
    using Fsel.Interaction.Domain.Enums;

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
