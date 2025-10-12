// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Configs
{
    using Fsel.Course.Domain.Entities.TestConfigs;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class TestAnswerEntityTypeConfiguration : IEntityTypeConfiguration<TestAnswer>
    {
        public void Configure(EntityTypeBuilder<TestAnswer> builder)
        {
            builder.HasOne(a => a.TestSectionResult)
                   .WithMany(b => b.TestAnswers)
                   .HasForeignKey(p => p.TestSectionResultId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(a => a.Question)
                   .WithMany(b => b.TestAnswers)
                   .HasForeignKey(p => p.QuestionId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
