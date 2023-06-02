// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Configs
{
    using Fsel.Course.Domain.Entities;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;
    using Microsoft.EntityFrameworkCore;

    public class SectionQuestionEntityTypeConfiguration : IEntityTypeConfiguration<SectionQuestion>
    {
        public void Configure(EntityTypeBuilder<SectionQuestion> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);

            builder.HasOne(a => a.Question)
                .WithMany(b => b.SectionQuestions)
                .HasForeignKey(b => b.QuestionId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(a => a.Section)
                .WithMany(b => b.SectionQuestions)
                .HasForeignKey(b => b.SectionId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(a => a.SectionPart)
               .WithMany(b => b.SectionQuestions)
               .HasForeignKey(b => b.SectionPartId)
               .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
