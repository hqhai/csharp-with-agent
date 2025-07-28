// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Configs
{
    using System;
    using Fsel.Common.Helpers;
    using Fsel.Course.Domain.Entities;
    using Fsel.Shared.Enums;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class QuestionExplanationErrorEntityTypeConfiguration : IEntityTypeConfiguration<QuestionExplanationError>
    {
        public void Configure(EntityTypeBuilder<QuestionExplanationError> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);
            builder.Property(e => e.Status)
                 .HasMaxLength(100)
                 .HasConversion(
                     v => v.ToString(),
                     v => v.EnumParse<EnumProcessedStatus>());

            builder.Property(e => e.FeedbackExplanation)
                .HasMaxLength(100)
                .HasConversion(
                    v => v.ToString(),
                    v => v.EnumParse<EnumFeedbackExplanation>());

            builder.Property(e => e.ExplanationType)
             .HasMaxLength(100)
             .HasConversion(
                 v => v.ToString(),
                 v => v.EnumParse<EnumFeatureExplanationType>());

            builder.HasOne(a => a.Question)
                  .WithMany(b => b.QuestionExplanationErrors)
                  .HasForeignKey(b => b.QuestionId)
                  .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
