// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Configs
{
    using Fsel.Course.Domain.Entities;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class MockTestAnswerEntityTypeConfiguration : IEntityTypeConfiguration<MockTestAnswer>
    {
        public void Configure(EntityTypeBuilder<MockTestAnswer> builder)
        {
            builder.HasOne(a => a.SectionQuestion)
                .WithMany(b => b.MockTestAnswers)
                .HasForeignKey(b => b.SectionQuestionId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(a => a.MockTestResult)
                .WithMany(b => b.MockTestAnswers)
                .HasForeignKey(b => b.MockTestResultId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
