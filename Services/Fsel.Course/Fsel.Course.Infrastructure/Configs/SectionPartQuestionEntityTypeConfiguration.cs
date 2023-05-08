// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Configs
{
    using Fsel.Course.Domain.Entities;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;
    using Microsoft.EntityFrameworkCore;

    public class SectionPartQuestionEntityTypeConfiguration : IEntityTypeConfiguration<SectionPartQuestion>
    {
        public void Configure(EntityTypeBuilder<SectionPartQuestion> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);

            builder.HasOne(a => a.Question)
                .WithMany(b => b.SectionPartQuestions)
                .HasForeignKey(b => b.QuestionId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(a => a.Section)
                .WithMany(b => b.SectionPartQuestions)
                .HasForeignKey(b => b.SectionId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
