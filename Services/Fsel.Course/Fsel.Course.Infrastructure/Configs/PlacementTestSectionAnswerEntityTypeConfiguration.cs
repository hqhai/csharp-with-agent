// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Configs
{
    using System;
    using Fsel.Course.Domain.Entities;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class PlacementTestSectionAnswerEntityTypeConfiguration : IEntityTypeConfiguration<PlacementTestAnswer>
    {
        public void Configure(EntityTypeBuilder<PlacementTestAnswer> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);

            builder.HasOne(a => a.PlacementTestSectionResult)
                .WithMany(b => b.PlacementTestAnswers)
                .HasForeignKey(b => b.PlacementTestSectionResultId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(a => a.SectionQuestion)
                .WithMany(b => b.PlacementTestAnswers)
                .HasForeignKey(b => b.SectionQuestionId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
