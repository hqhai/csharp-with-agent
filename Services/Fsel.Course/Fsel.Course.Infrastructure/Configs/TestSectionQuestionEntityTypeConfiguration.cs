// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Configs
{
    using System;
    using Fsel.Course.Domain.Entities.TestConfigs;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class TestSectionQuestionEntityTypeConfiguration : IEntityTypeConfiguration<TestSectionQuestion>
    {
        public void Configure(EntityTypeBuilder<TestSectionQuestion> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);

            builder.HasOne(a => a.Question)
                   .WithMany(b => b.TestSectionQuestions)
                   .HasForeignKey(p => p.QuestionId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(a => a.TestSection)
                 .WithMany(b => b.TestSectionQuestions)
                 .HasForeignKey(p => p.TestSectionId)
                 .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
